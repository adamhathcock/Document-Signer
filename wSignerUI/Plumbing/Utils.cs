using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace wSignerUI
{
    public static class Utils
    {
        public static string GetPropertyName<TProp>(Expression<Func<TProp>> propertyExpr)
        {
            MemberExpression memberExpr;
            return propertyExpr != null
                   && propertyExpr.Body.NodeType == ExpressionType.MemberAccess
                   && (memberExpr = propertyExpr.Body as MemberExpression) != null
                       ? memberExpr.Member.Name
                       : null;
        }

        public static BitmapSource GetBitmapSourceIconFromFile(string filePath)
        {
            BitmapSource result;
            using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
            using (var bmp = icon.ToBitmap()) { 
                var hBitmap = bmp.GetHbitmap();
                try {
                    result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); 
                } 
                finally
                {
                    NativeMethods.DeleteObject(hBitmap);
                } 
            }
            return result;
        }
    }
}