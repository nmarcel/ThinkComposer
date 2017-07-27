using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Web;

namespace AdminUtils
{
    /// <summary>
    /// Submits post data to a url.
    /// </summary>
    public class WebPostSubmitter
    {
        /// <summary>
        /// determines what type of post to perform.
        /// </summary>
        public enum EPostKind
        {
            /// <summary>
            /// Does a get against the source.
            /// </summary>
            Get,
            /// <summary>
            /// Does a post against the source.
            /// </summary>
            Post
        }

        private string PostUrl = string.Empty;
        private NameValueCollection PostValues = new NameValueCollection();
        private EPostKind PostKind = EPostKind.Get;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WebPostSubmitter()
        {
        }

        /// <summary>
        /// Constructor that accepts a url as a parameter
        /// </summary>
        /// <param name="Url">The url where the post will be submitted to.</param>
        public WebPostSubmitter(string Url)
            : this()
        {
            PostUrl = Url;
        }


        /// <summary>
        /// Constructor allowing the setting of the url and items to post.
        /// </summary>
        /// <param name="Url">the url for the post.</param>
        /// <param name="Values">The values for the post.</param>
        public WebPostSubmitter(string Url, NameValueCollection Values)
            : this(Url)
        {
            PostValues = Values;
        }


        /// <summary>
        /// Gets or sets the url to submit the post to.
        /// </summary>
        public string Url
        {
            get
            {
                return PostUrl;
            }
            set
            {
                PostUrl = value;
            }
        }
        /// <summary>
        /// Gets or sets the name value collection of items to post.
        /// </summary>
        public NameValueCollection PostItems
        {
            get
            {
                return PostValues;
            }
            set
            {
                PostValues = value;
            }
        }
        /// <summary>
        /// Gets or sets the type of action to perform against the url.
        /// </summary>
        public EPostKind Kind
        {
            get
            {
                return PostKind;
            }
            set
            {
                PostKind = value;
            }
        }
        /// <summary>
        /// Posts the supplied data to specified url.
        /// </summary>
        /// <returns>Indication of success, plus response or error message.</returns>
        public Tuple<bool,string> Post()
        {
            var Parameters = new StringBuilder();
            for (int i = 0; i < PostValues.Count; i++)
            {
                EncodeAndAddItem(ref Parameters, PostValues.GetKey(i), PostValues[i]);
            }
            var Result = PostData(PostUrl, Parameters.ToString());
            return Result;
        }
        /// <summary>
        /// Posts the supplied data to specified url.
        /// </summary>
        /// <param name="Url">The url to post to.</param>
        /// <returns>Indication of success, plus response or error message.</returns>
        public Tuple<bool,string> Post(string Url)
        {
            PostUrl = Url;
            return this.Post();
        }
        /// <summary>
        /// Posts the supplied data to specified url.
        /// </summary>
        /// <param name="Url">The url to post to.</param>
        /// <param name="Values">The values to post.</param>
        /// <returns>Indication of success, plus response or error message.</returns>
        public Tuple<bool, string> Post(string Url, NameValueCollection Values)
        {
            PostValues = Values;
            return this.Post(Url);
        }
        /// <summary>
        /// Posts data to a specified url. Note that this assumes that you have already url encoded the post data.
        /// </summary>
        /// <param name="PostData">The data to post.</param>
        /// <param name="Url">the url to post to.</param>
        /// <returns>Returns indication of sucess, plus the response of the post (on sucess) or error message (on failure).</returns>
        private Tuple<bool,string> PostData(string Url, string PostData)
        {
            HttpWebRequest Request = null;
            if (PostKind == EPostKind.Post)
            {
                Uri Address = new Uri(Url);
                Request = (HttpWebRequest)WebRequest.Create(Address);
                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded";
                Request.ContentLength = PostData.Length;
                using (Stream writeStream = Request.GetRequestStream())
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] bytes = encoding.GetBytes(PostData);
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            else
            {
                Uri Address = new Uri(Url + "?" + PostData);
                Request = (HttpWebRequest)WebRequest.Create(Address);
                Request.Method = "GET";
            }

            var Success = true;
            var Response = string.Empty;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)Request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                            Response = readStream.ReadToEnd();
            }
            catch (Exception Problem)
            {
                Success = false;
                Response = Problem.Message;

                /* Pending... add more useful data if any
                var WebProblem = Problem as WebException;
                if (WebProblem != null)
                {
                    Response = Response + Environment.NewLine + WebProblem.x
                } */
            }

            return Tuple.Create(Success, Response);
        }
        /// <summary>
        /// Encodes an item and ads it to the string.
        /// </summary>
        /// <param name="BaseRequest">The previously encoded data.</param>
        /// <param name="DataItem">The data to encode.</param>
        /// <returns>A string containing the old data and the previously encoded data.</returns>
        private void EncodeAndAddItem(ref StringBuilder BaseRequest, string Key, string DataItem)
        {
            if (BaseRequest == null)
            {
                BaseRequest = new StringBuilder();
            }
            if (BaseRequest.Length != 0)
            {
                BaseRequest.Append("&");
            }
            BaseRequest.Append(Key);
            BaseRequest.Append("=");
            BaseRequest.Append(HttpUtility.UrlEncode(DataItem));
        }
    }
}
