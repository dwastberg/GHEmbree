using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GHEmbree
{
    public class GHEmbreeInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "GHEmbree";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("8cd4f081-7c62-45d2-868c-1ec1bc06a2f0");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
