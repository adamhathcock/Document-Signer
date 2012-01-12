using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace wSignerUI
{
    public class ViewModelBase:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged<TProp>(Expression<Func<TProp>> propertyExpr)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var propName = Utils.GetPropertyName(propertyExpr);
                if(propName!=null)
                {
                    handler(this, new PropertyChangedEventArgs(propName));
                }
            }
        }
    }
}