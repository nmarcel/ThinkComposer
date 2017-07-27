using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Instrumind.Common;

using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.Common.Visualization;

namespace Instrumind.ThinkComposer.MetaModel.Configurations
{
    /// <summary>
    /// References an Idea to be selected for later Composition/Domain Merge.
    /// </summary>
    public class SchemaMemberSelection : INotifyPropertyChanged
    {
        private static Dictionary<Type, MemberAccesor> RegisteredMemberTypeOperationsSets = new Dictionary<Type, MemberAccesor>();

        public static void ClearRegisteredMemberTypeOperationsSets()
        {
            RegisteredMemberTypeOperationsSets.Clear();
        }

        public static void RegisterMemberTypeOperationsSet<TMember>(Func<TMember, IEnumerable<object>> ChildrenGetterOp,
                                                                    Func<TMember, string> NameCaptionGetterOp,
                                                                    Func<TMember, string> SummaryGetterOp = null,
                                                                    Func<TMember, ImageSource> PictogramGetterOp = null,
                                                                    Func<TMember, string> DescriptiveCaptionGetterOp = null,
                                                                    Func<TMember, ImageSource> DefinitorPictogramGetterOp = null)
                     where TMember : class
        {
            General.ContractRequiresNotNull(ChildrenGetterOp, NameCaptionGetterOp);

            var Accesor = new MemberAccesor { ChildrenGetter = (obj => ChildrenGetterOp((TMember)obj)),
                                              NameCaptionGetter = (obj => NameCaptionGetterOp((TMember)obj)),
                                              SummaryGetter = (obj => SummaryGetterOp.NullDefault(ob => null)((TMember)obj)),
                                              PictogramGetter = (obj => PictogramGetterOp.NullDefault(ob => null)((TMember)obj)),
                                              DescriptiveCaptionGetter = (obj => DescriptiveCaptionGetterOp.NullDefault(ob => null)((TMember)obj)),
                                              DefinitorPictogramGetter = (obj => DefinitorPictogramGetterOp.NullDefault(ob => null)((TMember)obj)) };

            RegisteredMemberTypeOperationsSets.AddOrReplace(typeof(TMember), Accesor);
        }

        public static MemberAccesor GetAccesor(object Member)
        {
            var MemberType = Member.GetType();
            foreach (var OperationsSet in RegisteredMemberTypeOperationsSets)
                if (OperationsSet.Key.IsAssignableFrom(MemberType))
                    return OperationsSet.Value;

            return null;
        }

        public static SchemaMemberSelection CreateSelectionTree(object SourceObject, MemberAccesor SourceAccesor = null, IEnumerable<object> PreExclusion = null,
                                                                Func<object, Tuple<bool,      // is-selectable
                                                                                   bool,      // is-selected
                                                                                   bool,      // can-show-is-selected
                                                                                   IRecognizableElement>    // action-info
                                                                    > MemberMergeSelectionDeterminer = null,
                                                                Func<object, MemberAccesor, bool> ChildrenFilter = null,  // Determines whether a child should be included for selection (e.g. "obj is Concept || obj.CompositeIdeas.Count >0")
                                                                SchemaMemberSelection DirectParent = null)
        {
            if (SourceAccesor == null)
                SourceAccesor = GetAccesor(SourceObject);

            if (SourceAccesor == null)
                return null;

            var Selection = (MemberMergeSelectionDeterminer == null
                             ? Tuple.Create(false, false, false, (IRecognizableElement)null)
                             : MemberMergeSelectionDeterminer(SourceObject));

            var Result = new SchemaMemberSelection(SourceObject, SourceAccesor)
            {
                RefMember = SourceObject,
                IsSelectable = Selection.Item1,
                IsSelected = (Selection.Item2
                              && (PreExclusion == null || !PreExclusion.Contains(SourceObject))),
                CanShowIsSelected = Selection.Item3,
                ActionInfo = Selection.Item4,
                Parent = DirectParent
            };

            var SourceChildren = Result.MemberChildren;
            if (!SourceChildren.IsEmpty())
            {
                Result.Children = new List<SchemaMemberSelection>();
                foreach (var Child in SourceChildren)
                {
                    var ChildAccesor = GetAccesor(Child);
                    if (ChildrenFilter == null || (ChildAccesor != null && ChildrenFilter(Child, ChildAccesor)))
                    {
                        var SelItem = CreateSelectionTree(Child, ChildAccesor, PreExclusion,
                                                                 MemberMergeSelectionDeterminer, ChildrenFilter, Result);
                        if (SelItem != null)
                            Result.Children.Add(SelItem);
                    }
                }
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public SchemaMemberSelection(object RefMember, MemberAccesor Accesor)
        {
            this.RefMember = RefMember;
            this.Accesor = Accesor;
        }

        public object RefMember { get; private set; }

        public SchemaMemberSelection Parent { get; set; }

        public IList<SchemaMemberSelection> GetContainmentHierarchy(bool IncludeThisInstance = false, bool IncludeRoot = true)
        {
            var Hierarchy = new List<SchemaMemberSelection>();
            var DirectParent = (IncludeThisInstance ? this : Parent);

            while (DirectParent != null)
            {
                Hierarchy.Add(DirectParent);
                DirectParent = DirectParent.Parent;
            }

            Hierarchy.Reverse();
            if (!IncludeRoot &&  Hierarchy.Count > 0)
                Hierarchy.RemoveAt(0);

            return Hierarchy;
        }

        // This should be one for the entire tree
        public bool IsPointed
        {
            get { return this.IsPointed_; }
            set
            {
                this.IsPointed_ = value;

                this.NotifyPropertyChange("IsPointed");
            }
        }
        private bool IsPointed_ = false;

        public bool IsSelectable { get; set; }

        public bool IsSelected
        {
            get { return this.IsSelected_; }
            set
            {
                value = (this.IsSelectable && value);
                if (this.IsSelected_ == value)
                    return;

                this.IsSelected_ = value;
                this.NotifyPropertyChange("IsSelected");

                if (this.Parent != null)
                    this.Parent.NotifyPropertyChange("AreChildrenSelected");
            }
        }
        private bool IsSelected_ = false;

        public bool CanShowIsSelected { get; set; }

        public IRecognizableElement ActionInfo { get; set; }

        public bool HasChildren { get { return (this.Children != null && this.Children.Count > 0); } }

        public bool CanSelectChildren { get { return (this.HasChildren && this.CanShowIsSelected); } }

        public bool AreChildrenSelected
        {
            get
            {
                if (this.Children == null || this.Children.Count < 1)
                    return false;

                // Notice that only one unselected (selectable) child makes this flag false
                var Result = !this.Children.Any(chld => chld.IsSelectable && !chld.IsSelected);
                return Result;
            }
            set
            {
                if (this.Children != null && this.Children.Count > 0)
                    foreach (var Child in this.Children)
                        if (Child.IsSelectable)
                        {
                            Child.IsSelected = value;
                            Child.AreChildrenSelected = value;
                        }

                this.NotifyPropertyChange("AreChildrenSelected");
            }
        }

        public IList<SchemaMemberSelection> Children { get; set; }

        /// <summary>
        /// Gets the current selection (key=selected-item, value=selected-item-parent)
        /// </summary>
        public Dictionary<SchemaMemberSelection, SchemaMemberSelection> GetSelection(Nullable<bool> FilterIsSelected = null)
        {
            var Result = new Dictionary<SchemaMemberSelection, SchemaMemberSelection>();

            this.GetCompleteTree(Result);

            if (FilterIsSelected != null)
                Result = Result.Where(sel => sel.Key.IsSelected == FilterIsSelected.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return Result;
        }

        private void GetCompleteTree(Dictionary<SchemaMemberSelection, SchemaMemberSelection> Result, SchemaMemberSelection Parent = null)
        {
            Result.AddNew(this, Parent);

            if (this.Children != null && this.Children.Count > 0)
                foreach (var Child in Children)
                    Child.GetCompleteTree(Result, this);
        }

        public void ApplySelector(Func<object, bool?> Selector)
        {
            var Selected = Selector(this.RefMember);
            if (Selected != null)
                this.IsSelected = Selected.Value;

            if (Children != null)
                foreach (var Child in Children)
                    Child.ApplySelector(Selector);
        }

        public void NotifyPropertyChange(string PropertyName)
        {
            var Handler = this.PropertyChanged;
            if (Handler == null)
                return;

            Handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        public SchemaMemberSelection FindEquivalentCounterpartAtSameLevel(SchemaMemberSelection CounterpartRoot, Func<object, object, bool> MatchEvaluator)
        {
            var LocalHierarchy = this.GetContainmentHierarchy(true, false);    // Notice that the root is ignored
            var CounterpartSelection = CounterpartRoot;

            foreach (var Selection in LocalHierarchy)
            {
                if (!CounterpartSelection.HasChildren)
                    return null;

                CounterpartSelection = CounterpartSelection.Children.FirstOrDefault(chl => MatchEvaluator(chl.RefMember, Selection.RefMember));
                if (CounterpartSelection == null)
                    break;
            }

            return CounterpartSelection;
        }

        public SchemaMemberSelection FindInChildrenHierarchy(Func<SchemaMemberSelection, bool> MatchEvaluator)
        {
            if (MatchEvaluator(this))
                return this;

            if (this.Children != null)
                foreach (var Child in this.Children)
                {
                    var Result = Child.FindInChildrenHierarchy(MatchEvaluator);
                    if (Result != null)
                        return Result;
                }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private MemberAccesor Accesor { get; set; }

        public IEnumerable<object> MemberChildren { get { return this.Accesor.ChildrenGetter(this.RefMember); } }
        public string MemberNameCaption { get { return this.Accesor.NameCaptionGetter(this.RefMember); } }
        public string MemberSummary { get { return this.Accesor.SummaryGetter(this.RefMember); } }
        public ImageSource MemberPictogram { get { return this.Accesor.PictogramGetter(this.RefMember); } }
        public string MemberDescriptiveCaption { get { return this.Accesor.DescriptiveCaptionGetter(this.RefMember); } }
        public ImageSource MemberDefinitorPictogram { get { return this.Accesor.DefinitorPictogramGetter(this.RefMember); } }

        public override string ToString() { return "Member=[" + this.MemberNameCaption.ToStringAlways() + "]"; }

        public class MemberAccesor
        {
            public Func<object, IEnumerable<object>> ChildrenGetter { get; set; }
            public Func<object, string> NameCaptionGetter { get; set; }
            public Func<object, string> SummaryGetter { get; set; }
            public Func<object, ImageSource> PictogramGetter { get; set; }
            public Func<object, string> DescriptiveCaptionGetter { get; set; }
            public Func<object, ImageSource> DefinitorPictogramGetter { get; set; }
        }
    }
}
