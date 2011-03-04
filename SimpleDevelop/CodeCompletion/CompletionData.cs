using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    class CompletionData : ICompletionData
    {
        private static ConcurrentDictionary<MemberTypes, ImageSource> ImageSources;
        protected static ImageSource ConstantImage;
        protected static ImageSource EnumImage;
        protected static ImageSource InterfaceImage;
        protected static ImageSource StructImage;

        static CompletionData()
        {
            ImageSources = new ConcurrentDictionary<MemberTypes, ImageSource>();
        }

        // Hack! Needs to be called from the UI thread (oh well)...
        public static void Initialize()
        {
            ImageSources[MemberTypes.Event] = BitmapToImageSource(Properties.Resources.Event);
            ImageSources[MemberTypes.Field] = BitmapToImageSource(Properties.Resources.VariablePublic);
            ImageSources[MemberTypes.Method] = BitmapToImageSource(Properties.Resources.MethodPublic);
            ImageSources[MemberTypes.NestedType] = BitmapToImageSource(Properties.Resources.ClassPublic);
            ImageSources[MemberTypes.Property] = BitmapToImageSource(Properties.Resources.PropertyPublic);

            ConstantImage = BitmapToImageSource(Properties.Resources.ConstantPublic);
            EnumImage = BitmapToImageSource(Properties.Resources.EnumPublic);
            InterfaceImage = BitmapToImageSource(Properties.Resources.Interface);
            StructImage = BitmapToImageSource(Properties.Resources.Struct);
        }

        protected static ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
        }

        public CompletionData(MemberInfo memberInfo)
        {
            Text = memberInfo.Name;

            ImageSource imageSource;
            Image = ImageSources.TryGetValue(memberInfo.MemberType, out imageSource) ? imageSource : null;
        }

        public string Text { get; private set; }

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
