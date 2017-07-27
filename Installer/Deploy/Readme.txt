Instrumind ThinkComposer v1.5.1604
==================================

Copyright (C) 2011-2016 Néstor Marcel Sánchez Ahumada

Open-source project site: http://thinkcomposer.codeplex.com
Find the author at: nestormarcel@gmail.com

--------------------------------------------------------------------------------------
ThinkComposer is free software licensed under the GNU General Public License.
It is provided without any warranty. You should find a copy of the license in the root directory of this software product.

--------------------------------------------------------------------------------------
SYSTEM REQUIREMENTS:
- Processor       : x86-compatible at 2.2 GHz.
- Memory          : 2 GB of RAM.
- Disk Space      : 500MB free.
- Operating System: Windows 7, Vista, XP (SP 3) or later compatible with Windows Presentation Foundation (WPF).
- Video Card      : VGA compatible with minimum resolution of 1024x768 pixels, 1600x1200 or more is recommended.

The Microsoft .NET Framework 4.0 must be preinstalled.
If not already present in your PC, you can download it from...
http://www.microsoft.com/download/en/details.aspx?displaylang=en&id=17851

--------------------------------------------------------------------------------------

VERSIONS HISTORY:
*****************
Version 1.5.1604;
- Fixed: Name of a global Detail designator is no longer changed when editing an empty link after trying to access it.
- Fixed: Views can be exported as image in PDF format again.
- Fixed: Application crashes when Finding text (the problem is detected and crash prevented).

Version 1.5.1502;
- === SINCE FEBRUARY-2015 THINKCOMPOSER IS OPEN-SOURCE SOFTWARE ===
- PDF generation is now based on an open-source library (PDFsharp) instead of a commercial product.
- Fixed: Link descriptor are lost for new links after copy+paste.

Version 1.5.1230;
- New: Default grid size now can be specified in Domain properties.
- New: While creating new Concepts, by dropping their Definition from the Concepts palette, press [Ctrl-Left] to open the properties editor instead of just edit the name/title over the symbol.
- Changed: Markers palette display order is changed to show User-Defined Markers on top.
- Fixed: Change to Symbol Format properties 'Flip Horizontal/Vertical' does not propagate instantly.
- Fixed: Crash when trying to attach notes to Ideas.

Version 1.5.1202;
- Fixed: Dialog (such as Concept Definition) crash when open in second screen in a dual monitor environment.

Version 1.5.1149;
- Fixed: The content tree no longer show changes after deleting an Idea.

Version 1.5.1127;
- New Domains: Argumentative mapping and Influence diagram.
- New: Tilt command to turn selected symbols 90º clockwise (flip them to change orientation).
- New: Show as Multiple command to show selected symbols as multiple stacked ones.
- Fixed: Cursor is incorrectly positioned, after inserting text with line-break or tab characters, in symbol Name/Title.
- Fixed: Incorrect selection indicators are shown after deleting objects.
- Fixed: Visual objects remains orphan after deleting a Relationship.
- Fixed: Crash when setting Line color brush as none.

Version 1.5.1104;
- New: Auto-size based on entered text (active by default on new Compositions. Activate/deactivate in View -> "Auto-Size by Entered Text").
- Improved: While editing an object's text in the View, press [Alt]+[Enter] to insert a new-line, or press [Shift]+[Tab] to insert a tab character.
            Also, the text input can be always finished by pressing [Ctrl]+[Enter].
- Improved: Move selected objects by pressing Arrow-Keys using steps of View's Grid size (press [Ctrl] to force steps of 1 pixel, and [Shift] to multiply those steps by 4).
- Fixed: Toolbar does not show buttons when pressing collapse/expand pin repeatedly.
- Fixed: While displacing a connector, the handle icon is shown near the target symbol's center instead of the connector's center.
- Fixed: Symbols with Fixed-Size can be resized on creation by dragging (they should not).

Version 1.4.4022;
- Improved: Toolbar is collapsed immediately when pressing the collapse/expand pin.
- Improved: A context-menu, with commands depending on the selected object type,
            is displayed when clicking with the Right/Alt mouse button, over the Content Tree.
- Improved: Views can be deleted from the context-menu shown in the Content Tree.
- Improved: A new predefined output-template has been defined for export "Propositions" (useful in Concept Mapping) from Relationships.
            Apply it in the "Composition" text Output-Template of the Domain, plus clearing the Concepts and Relationships base templates, and then go to Tools->Generate files.
- Changed: In Relationship creation, the pointed Idea can be designated as 'Origin' if pressing [Left-ALT]. Previously, any [ALT] key (left of right) was accepted.
- Changed: In Relationship creation, to insert an intermediate Relationship (applycable only for non-simple Relationships), now you must press the [Right-ALT] key instead of [Ctrl].
- Fixed: Window maximization or resize does cross the area of Windows taskbar, making inaccessible some buttons and other controls.
- Fixed: Crashed when undoing creation of Composite Content View.
- Fixed: Deletion of Ideas does not close Views of affected composite/nested Ideas.
- Fixed: Edit text-box remains active while Undoing/Redoing commands.
- Fixed: Undo of automatic created Concepts (like in the 'Mind Mapping' Domain) is partially applied by object (i.e. first Concept, then Relationship), instead as applied to the whole command.
- Fixed: Apply format does not works from and to Complements.
- Fixed: Crashes with a simple relationship, just after setting the center on empty-space, while using the ongoing target creator to point precisely (presing [Ctrl]) to the target symbol.

Version 1.4.3928;
- Improved: New symbol geometry shapes added (arrows and double-arrows).
- Improved: Widened Shape selector in Idea-Definitions.
- Improved: Brush color selector now have fields for entering RGB values and the fine selection range is adjusted on each selection.
- Fixed: Brush color selector appears with an unused blank area at the right side.
- Fixed: File/Code Generation sometimes crashes when Ideas have custom brushes.

Version 1.4.3909;
- New: Table Details now can contain Pictures as specified in Table-Structure field definitions.
- Improved: Extended palette of predefined Styles.
- Fixed: Paste tool works as Paste Shortcut.

Version 1.4.3826;
- New: Drag and drop of files from outside ThinkComposer into the diagram (works as Paste).
- New: Idea Definitions can be grouped in clusters, improving their organization. Create the Clusters in the Domain definition.
- Improved: HTML Reports now include the current path/route, plus navigation links, at the top of each page (labeled as "At: ").
- Improved: Paste of text delimited by tab or comma, suchas as that copied from Excel, is now pasted as a Table detail (must select the "as detail" check-box).
- Improved: Paste now can create Link Details when pasting text starting with "http://" (must select the "as detail" check-box).
- Improved: A Relationship from another Relationship can be created while dragging the pointing connector and pressing [Ctrl].
- Fixed: Export Image fails to save it as PDF.
- Fixed: Formats applied to individual Ideas are loss for undo/redo.
- Fixed: After pressing [ALT] the diagram editor losses the focus.
- Fixed: Related origins are not shown/collapsed when pressing the Related icon in the symbol selector and [ALT-Left]. 
- Fixed: Some predefined Styles have transparent background (instead of white), producing display problems.

Version 1.4.3730;
- New: Content-tree now presents a box to Find Ideas by text within their Name.
- Improved: Concepts can be created also by dragging the mouse after dropping a Concept Definition, thus giving a custom initial size.
- Improved: Extra origin links can be created by pressing [ALT] while dragging the mouse from the center of the Relationship symbol.
- Fixed: Pointing not using Snap-To-Grid works incorrectly.

Version 1.3.3721;
- Fixed: Instability at startup on x64 machines.
- Fixed: Deleted Idea remains in content-tree and application crashes when clicking at it.

Version 1.3.3714;
- New: Output-Templates can inject sub-templates with optional identation (for recursive generation).
- Improved: New symbol geometry shapes added (bin, button, cloud, funnel, pentagon, rect-crossed-top, rect-diagonal, wrapper).
- Improved: Find and Replace for (Rich-Text) Descriptions, Tech-Specs and Output-Templates.
- Improved: Content tree now is sortable by Name or Creation sequence, alternately (always Views goes first).
- Improved: After creating a shortcut, all other selected objects are unselected.
- Fixed: Symbol's Idea Definition label adorner should be movable when shown without the symbol (on simple Relationships hidding their central symbol).
- Fixed: Crash when trying to generate files from a new Composition.

Version 1.3.3528;
- Fixed: Button "Apply", intended only for the File Generation Configuration saving, appears on all edit windows (not applying changes).
- Fixed: Marker assignments direct edit (over Idea's symbol) only works when details are shown.

Version 1.3.3522;
- New: File generation from Ideas, based on text Output-Templates using the "Liquid" format. Very useful for code generation and custom data exports.
- New: Domains can declare External Languages, to base Output-Templates -for file/code generation- on them.
- New: Domains can declare generic Output-Templates at Composition, Concepts and Relationships levels.
- New: Idea Definitions can declare Output-Templates (ad-hoc to their particularities) and optionally extend its base Output-Template.
- New: Text editor for Tech-Specs and Output-Templates with: Line numbering, auto-indenting and syntax coloring (currently only available for Output-Templates editing).
- Improved: Marker assignments can be edited just clicking over them (while shown over an Idea Symbol).
- Improved: In the Detail Link editor window, now a drop-down list of recently visited websites is shown.
- Fixed: Relink fails over Ideas based on Idea-Definitions having Origin/Target Roles linkability restrictions.
- Fixed: Snap-to-Grid feature not working over precise grid positions.
- Fixed: When Relationship is Simple and hidding Main-Symbol, then is no way to edit Relationship properties. Now you can right-click on a connector and select "Relationship Properties".
- Fixed: Cannot select Relationship by pointing over its name Label (when the symbol is hidden).
- Fixed: Visually misplaced Connector selector (in yellow color) after Connector displaced.
- Fixed: Crash in Search or Replace if scanning the Description property.

Version 1.2.3219;
- New: A Table cell can contain a whole table inside. These new "Nested" Tables are created on Fields based on the new Table data-type.
- Improved: Legend complements now also shows the Variants of Relationship Definitions.
- Improved: No more sudden auto-resize of windows depending of their content.
- Improved: Application error window provides a way to report it to Instrumind Software.
- Fixed: Replace text causes corruption of Ideas' Description properties (in its RTF+XML format).
- Fixed: Reports (all types) does not show the Pictogram of the Domain's Definitions.
- Fixed: Advanced properties are not always shown when required (in various edit windows). 
- Fixed: Exceeding numeric field limits (such as allowed decimal digits) in Table manual edit, makes the typed value to be overwritten by the stored value.
- Fixed: Table editor for single-record, with form style, shows an empty list for fields based on the Switch data-type.
- Fixed: Moving an Symbol with auto-reference (having a Relationship pointing to itself) does not move the two Ideas along.

Version 1.2.1217;
- Fixed: The "Select all" tool crashes.
- Fixed: Connectors not repainted correctly after collapsing a Symbol Details Poster.
- Changed: No repositioning of connecting points inside a Symbol head as result of hide/show the Details Poster.

Version 1.2.1123;
- Improved: New Plug geometries are available.
- Improved: Plug is now visible while cycling-through-variants (not hidden by re-linking indicator).
- Fixed: Reports crashes when Composition has Markers without descriptor.
- Fixed: Reports show Marker's incorrect pictogram or empty definitor.
- Fixed: HTML Report generation fails if Composition names have non standard charactes (such as quotes). Now these characters are changed by standard ones.
- Fixed: Export of View's image suggests a file-name truncated at the final characters.
- Fixed: The "Line Format" command does not change thickness and dash style of the selected objects.

Version 1.2.1113;
- New: HTML Reports (in the Professional and superior editions).
- New: PDF Report "save as" option, also for export View images.
- Improved: Simplified Table creation and Custom-Fields edit (structure is editable in a tab).
- *NOTE*: Compositions saved with this version are not readable with previous ones.
          Therefore, if you share your Compositions, make sure the receiver also has the latest ThinkComposer version!
- Changed: Resize command is symmetric only with the [Alt-Left] key, and Move command locks positon only with the [Alt-Right] key.
- New: Predefined Domains (Flowchart, Class diagram, Use Case diagram and Sequence diagram).
- New: Group Line Complement and context-menu option to change its axis (useful as "Lifeline" in UML Sequence Diagrams).
- New: Quote Complement.
- New: Symbols' format can specify fixed width and height.
- New: A Table or Custom-Field can reference an Idea. Idea-Reference type available for declaring Table-Structure fields.
- New: A Table or Custom-Field cell allows the user to pick values from tables (for text and number fields) or from an Idea of the Composition (for text fields).
- New: When editing a Table-Structure, a Field Definition, of numeric or text types, enables users to pick values from a Source Base Table (records).
- New: When editing a Table-Structure, a Field Definition, of text type, enables users to reference an Idea of the Composition by selecting which property use to reference it.
- New: Idea Definition's Symbol Format can show the symbol As Multiple (with a stack of 3 shapes) and with a Separator line between Details.
- New: Edit a selected Idea by pressing [F4].
- Improved: Increased number of "Representative Shape" geometries to choose.
- Improved: Reports inform Details contained in Ideas.
- Improved: Reports inform Links associated to Ideas (origins, targets and companions/siblings) and Links implemented by Relationships.
- Improved: Reports inform Tech-Spec of Ideas.
- Improved: Content Tree now includes the Idea's pictogram (just after the Definition pictogram).
- Improved: New "Arrange" tab on Idea-Definition edit windows with properties about autocreation and grouping.
- Improved: An Idea now can have multiple Group-Regions.
- Improved: Line Dash-style selectors (combo-box) now shows a visual sample.
- Improved: Line Thickness now can be selected from a combo-box showing a visual sample.
- Improved: "Line color" and "Connector color" tool-bar buttons now edits "format": color/brush, plus thickness and dash-style (also for Complements).
- Improved: Idea Definition's Symbol Format now can specify Flipped Horizontally and Vertically states, plus Tilted 90° clockwise.
- Improved: New geometric shapes added.
- Improved: More space is provided (while editing and displaying in symbols) for data field of type Text-Long.
- Improved: Symbol samples, shown in the Concepts and Relationships Palette, now are presented with the correct aspect-ration between initial widht and height.
- Fixed: Links with Role-Type of "Participant" are incorrectly shown (the "Target" plug is still used).
- Fixed: Crash after creating Idea Definition clones or Field Definition clones.
- Fixed: View aiming position (to the current visible area of the diagram) is lost after save/open the Composition or change to other View.
- Fixed: Idea Details are not shown in the order they are declared (in the "Details" tab).
- Fixed: Background brush/color for Group-Regions of Idea-Definitions symbol format is not applied.
- Fixed: Applying particular style properties (colors, line thickness) to Symbols incorrectly also applies them to Connectors.
- Fixed: Creating a Relationship starts from a precise position when the source Idea Definitor has the "Precise Connect by default" option not checked.
- Fixed: Copied Ideas has its Complements in the same place as the original ones.
- Changed: Minimum size of a symbol now is 2x2.
- Other minor improvements and bug-fixes.

Version 1.1.0824;
- New: View background brush/color and background image (*). See View properties.
- New: Domain default View background brush/color and background image (*). See Domain properties.
- Improved: Features related to Idea Details:
    + Button (green-arrow with disk) for directly extract Content of Attachments and Tables.
    + Context-menu to access Details tab of Idea's Properties.
    + Go-to link now is called "Access content" and also is used to externally access Attachments (the OS will call the associated application).
- Improved: Export of Details Table allows append records to an existing file (not only overwrite).
- Improved: Dialogs for Open/Save files now remember the last used directory (per file type).
- Improved: If only one tab remains open and you close it, then the whole Composition is closed (currently it requires to have at least one tab open).
- Improved: Safer saving of documents. If a crash happens while saving your original file is preserved in a ".old" file.
- Fixed: A composite-content view does not immediately show its changes in its owner Idea (when showing the composite-content as detail).
- Fixed: Detail tables (and other data grids) have too little height to show complete letters.
- Fixed: Content Tree does not reflects visual style changes after an Idea Definition is edited.
- Fixed: View position is altered after switching to other Composition.
*: If the image is bigger than 500x500 then it is adjusted to fit in the View, else it is repeated/tiled.

Version 1.1.0810;
- Fixed: Crash when showing/editing Details linked to properties such as 'Summary'.
- New: Capability to update document (Composition or Domain) structure in order to support new features and improve stability.
- New: Connectors can be precisely positioned inside a symbol (pres [Ctrl]), not only from/to its center.
- New: Idea Definition's "Precise Connect by default" property: Indicates to connect from/to precise aimed positions inside the Symbol, by default, else from/to the Symbol center.
- Fixed: Last modifier cannot be changed in Versioning tab while editing object properties.
- Changed: Color schema of predefined Domains.
- Fixed: Connector Plug 'Triline-Circle' has the circle crossed by the connector's line.
- Improved: Color brushes now can be set to nothing/null from any drop-down color brush selector.

Version 1.1.0802;
- Fixed: The "Representative Shape" property shows only Rectangles while editing Concept or Relationship Definitions.
- Fixed: Crash when editing globally predefined Details (such as Custom-Fields) on a Converted Idea.
- Improved: Connector lines now can be straighten even when linking an auto-reference Relationship.

Version 1.1.0731;
- New: Convert tool to change the base Idea Definition type of Ideas.
- New: View properties editing allows change of the Grid Size.
- New: Idea's Context-Menu for edit Markers.
- Improved: Image exporting quality of JPEG files enhanced to 90% (for 100% quality use PNG).
- Improved: Initial size of the Domain selection window is proportional to the main window.
- Improved: Faster switching details on multiple selected ideas.
- Improved: If user clicks a View in the content tree, then it is opened if not current else its properties are edited.
- Improved: Naming of Views has been simplified.
- Fixed: Relationship Connectors are not updated when the Relationship Definition is changed (origin/participant or target plugs variants).
- Fixed: Crash when picking the same internal image (icon) twice.
- Fixed: Crash when copying a visual object not having background color.
- Fixed: Images exported have a light gray border partially surrounding them.
- New: Export data from Table details (from Table Editor) into tab or csv files, the later using Windows (regional setting) list separator*.
- Changed: Importing of csv files now uses the Windows (regional setting) list separator*.
*: It uses ";" to avoid conflict with decimal separator if the Windows list separator is comma.

Version 1.0.7701;
- New: Report Generation Configuration.
- New: Preview as graphic representation of Concept and Relationship Definitions.
- Autoresizing of windows is now constrained within screen.
- Controls adjusted for Concept Definition and Relationship Definition edit windows.
- Fixed: Autopositioning (i.e. for Mind-mapping editing style) losses text of nodes beyond primary level.

Version 1.0.7618;
- Fixed initialization of diagram view from upper-left corner instead of center.

Version 1.0.7502;
- Fixed crash while generating Composition Report.

Version 1.0.7430;
- Activated to be available in Trial mode and Free for non-commercial use.

Version 1.0.6615; Beta 2
- Expiration of Beta version is extended to June 1st of 2012.
- New: Count of created Ideas (Concepts + Relationships) and used Composability Depth Levels while editing Composition properties. 
- New: Count of created Idea Definitions (Concept Definitions + Relationship Definitions) while editing Domain properties.
- Changed: Disposal of unused memory is performed as soon as possible (intended for slow machines).

Version 1.0.6528; Beta 2
- Fixed: Hangs (later crashes) on overloaded/slow computers while trying to force edit-in-place.
- Fixed: After panning the diagram View, while pressing mouse-right/alt-button, it "jumps" when panning again.
- Fixed: Descriptor was not created in Idea's Markers editor.

Version 1.0.6525; Beta 2
- Improved: Report extended to show composite Ideas with its nested content.
- New: Report can be saved as XPS document.
- Fixed: Crash if trying to generate report for empty Composition.
- Fixed: Connector disappears when moveing symbol connected from a Relationship-central-symbol after undo.

Version 1.0.6502; Beta 2
- New: Very basic Reporting capability.
- Expiration of Beta version is extended to May 1st of 2012.

Version 1.0.6423; Beta 2
- Properties panel reduced to only show Interrelations in order to avoid freezing,
  by complex data changes propagation, on slow computers.
  As usual, the properties still can be accessed thru double click on Idea or pressing the "Edit" button.

Version 1.0.6407; Beta 2
- Expiration of Beta version is extended to April 1st of 2012.
- New: Tab for editing Tech-Spec (only visible when showing Advanced properties)
- Modified: From now, printing a View does not show header and footer (Composition and View names, respecitvely).
            For that purpose, users can include an Info-Card Complement in the diagram View.
- New: License activation.

Version 1.0.6315; Beta 2
- Improved: Resizing of Group Regions (boundaries) now is not limited by the associated owning Symbol.
- Improved: Shortcut link indicator now is shown/hidden from the View along with the other symbol indicators.
- Fixed: Crash when selecting Idea in Content Tree where all views are closed. Now at least one View must remain open.
- Fixed: Confusing visual selection of items from palettes.
- Fixed: Callouts are not moved along with the Symbols they point.
- Fixed: Incorrectly the Background brush is shown when changing Line brush.
- Fixed: Lost of Group Region colors in multi-select.
- Fixed: Crash when appending new Concepts through Automatic Creation and no Relationship Definition is assigned for that.
- Predefined Domains added: "Timeline" and "Web Environment".

Version 1.0.6307; Beta 2
- New: Can create and edit Concepts/Relationships/Markers Definitions from Palettes
- Fixed: Initial size of Relationships central/main symbol is not too big.
- Fonts installed on Windows XP
- Setup changes for support Windows XP
- Working directories changed
(now, elevated privileges are not required, except for writing on shared predefined files)

Version 1.0.6231; Beta 2
- New: Double-click over a Concept, Relationship or Marker, on the respective Palettes, for edit it Definition.
- New: Automatic increment of version sequence on changed objects, plus propagation to parents/owners hierarchy.
- New: Extends/reduces capacity and features depending on product Edition.
- Improved: Search and Replace now works with multiple occurrences of a searched text within a property.
- Improved: When working with Domains, the document-selector and quick-palette reflects that properly.

Version 1.0.5224; Beta 2
- New: Can create Shortcuts by dragging and Idea from the Content Tree.
- Improved: Actioners are suppressed when selecting objects and pressing [Ctrl], [Shift] or [Alt]
- Fixed: While creating new Concepts with shape "<none>" on the View, there is nothing being dragged by the mouse.
- Fixed: Wrong scale while displaying composite content as details (in the symbol's poster).

Version 1.0.5221; Beta 2
- New: Extend a Relationship by dragging from its Symbol Center (alternate to Edit In-Place).
- New: Find and Find & Replace commands.
- Improved: The "Go to Parent" command now also appears in the menu Toolbar.

Version 1.0.5217; Beta 2
- New: Relinking (existing links/connectors can be reassigned to/from a new target/origin)
- Fixed: Detection of new version partially performed.
- Fixed: Crash when working with modified symbol format fonts.
- Fixed: Not Snap to grid

......................................................................................
Version 1.0.5213; Beta 1
- Fixed: Crash when saving Compositions with modified symbol format colors.
- Fixed: Snap to grid
- Improved: Navigate clicking on TreeView items.
- New: Shows name, type and summary of pointed objects in status bar.

......................................................................................
Version 1.0.5210; Beta 1
- Improved: Context menus now includes Delete, Cut, Copy and Paste commands when appropriate.
- Improved: Link's Descriptor now can include a Pictogram.
- Improved: Visual objects format now is changed property by property and not as a whole.
- Fixed: Crash when deleting an Idea with composite-content.
- Fixed: Non-Directional Relationship still uses the Target role plug instead of only the Origin/Participant.
- Fixed: Same Separation did not work beyond second symbol (Vertical and Horizontal).
- Fixed: Cancel button on Edit Windows not always cancelled changes
- Fixed: New dimensions of object while resizing is correctly previsualized in ongoing manipulation.
- Fixed: Display of Connectors does not follow the Definitor's formatting.
- Fixed: Visually incorrect/displaced Editing when View is Snapping-to-Grid and Zoom is not 100%

......................................................................................
Version 1.0.5127; Beta 1
- New: Context Menus (shown with mouse right/secondary-button click)
- New: Symbols can be Flipped Horizontally or Vertically
- New: "All-Purpose" Domain (many Concepts and Relationships)
- New: "Fast-Food Cuisine" Domain (sample used in the Product Manual)
- Changed: The main menu Toolbar starts open.
- Changed: Ideas palette was divided in Concepts and Relationships palettes.
- Changed: Copy of Ideas to clipboard will only copies the graphics (press [Alt] to also copy text).
- Improved: Export Image will use a Transparent background (not white) if pressing [Ctrl],
            for target file types which supports transparency (such as .PNG).
- Fixed: Display ratio of Pictogram when shown in Symbols.
- Expiration of Beta version is extended to March 1st of 2012.

......................................................................................
Version 1.0.5112; Beta 1
- New: Separate to same horizontal/vertical distance between visual objects.
- Changed: View starts with Snap-To-Grid and Show-Grid actived.

......................................................................................
Version 1.0.5111; Beta 1
- New: Snap to Grid
- New: Grid can be Lines based or Points based.
- New: Grid size is definable
- Improved: Tool-Bar now scrolls only the "Compose" group tab, keeping the "Project" group tab on the left side.
- Improved: In Shortcut symbols the Edit-In-Place feature is changed for a Go To Target symbol.
- Fixed: The Edit-In-Place for Info-Card complements now edits the Composition properties.
- Fixed: The Edit-In-Place for Legend complements now edits the Domain properties.

......................................................................................
Version 1.0.5107; Beta 1
- Fixed: Donwload and Update works again.
- Fixed: Predefined Domains can be loaded and used in Composition creation.

......................................................................................
Version 1.0.5103; Beta 1
- New: Detects new version, downloads it and update.

......................................................................................
Version 1.0.5027; Beta 1
- Custom Fields editing: Now a Table-Record-Reference can be selected using a ComboBox.

......................................................................................
Version 1.0.5021; Beta 1
- Fixed crash while canceling a Field Definition editing.

......................................................................................
Version 1.0.5020; Beta 1
- Added new predefined Domains: Mind Mapping, Data Model, Genealogy Tree.
- Fixed crash while deleting multiple Ideas.
- Fixed designation Name change for global Details definitions.
- Improved nomenclature for Table-Structure Definitions.
- Improved cloning of items while on editing by grid.

......................................................................................
Version 1.0.5012; Beta 1
- Link-Role Variants are the new generic (and extensible) way of supporting Multiplicities/Cardinalities.
- Improved detection of unloadable corrupt files (no crash, just warn).
- Export and printing with incorrect visual complements display fixed.
- In-Place editing of symbols is now also invoked pressing [F2].

......................................................................................
Version 1.0.5011; Beta 1
- Assembly versioning fixed for updateable setup.

......................................................................................
Version 1.0.5010; Beta 1
- File open crash fixed (now exclusive access is not required).

......................................................................................
Version 1.0.5007; Beta 1
- PAD file added.
- Improved visual contrast in editing windows.

......................................................................................
Version 1.0.5004; Beta 1
* Public Beta release.
- Fixed bug: View's objects collection mantaining z-order.
- Fixed bug: Selected objects are unselected while moving too fast.

......................................................................................
Version 1.0.4825; Beta 0
* Internal Beta release.
- Domain saving and opening from.
- Base Tables support.

......................................................................................
Version 1.0.3730; Alpha
* Alpha release for internal testing.
- Concept Mapping feature.
- Details attaching.

--------------------------------------------------------------------------------------
[End]
