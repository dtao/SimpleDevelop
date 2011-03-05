using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace SimpleDevelop.CodeCompletion
{
    abstract class CompletionData
    {
        protected static ImageSource EventImage;
        protected static ImageSource FieldImage;
        protected static ImageSource MethodImage;
        protected static ImageSource NestedTypeImage;
        protected static ImageSource PropertyImage;
        protected static ImageSource ConstantImage;
        protected static ImageSource EnumImage;
        protected static ImageSource InterfaceImage;
        protected static ImageSource StructImage;
        protected static ImageSource NamespaceImage;

        // Hack! Needs to be called from the UI thread (oh well)...
        public static void Initialize()
        {
            EventImage = BitmapToImageSource(Properties.Resources.Event);
            FieldImage = BitmapToImageSource(Properties.Resources.VariablePublic);
            MethodImage = BitmapToImageSource(Properties.Resources.MethodPublic);
            NestedTypeImage = BitmapToImageSource(Properties.Resources.ClassPublic);
            PropertyImage = BitmapToImageSource(Properties.Resources.PropertyPublic);
            ConstantImage = BitmapToImageSource(Properties.Resources.ConstantPublic);
            EnumImage = BitmapToImageSource(Properties.Resources.EnumPublic);
            InterfaceImage = BitmapToImageSource(Properties.Resources.Interface);
            StructImage = BitmapToImageSource(Properties.Resources.Struct);
            NamespaceImage = BitmapToImageSource(Properties.Resources.Namespace);
        }

        protected static ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
        }
    }

    abstract class CompletionData<TMemberInfo> : CompletionData, ICompletionData where TMemberInfo : MemberInfo
    {
        protected readonly TMemberInfo _memberInfo;

        public CompletionData(TMemberInfo memberInfo)
        {
            _memberInfo = memberInfo;
        }

        public virtual string Text
        {
            get { return _memberInfo.Name; }
        }

        public ImageSource Image { get; protected set; }

        public virtual double Priority
        {
            get { return 1.0; }
        }

        public virtual object Content
        {
            get { return Text; }
        }

        public virtual object Description
        {
            get { return Text; }
        }

        public virtual void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}
