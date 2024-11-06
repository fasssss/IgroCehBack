using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Attributes
{
    public class OrderAttribute: Attribute
    {
        public OrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }
    }
}
