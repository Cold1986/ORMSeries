﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Entity
{
    //书
    public class Book
    {
        public Book()
        {
            Reviews = new List<BookReview>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<BookReview> Reviews { get; set; }
        public override string ToString()
        {
            return string.Format("[{0}]------《{1}》", Id, Name);
        }
    }
}
