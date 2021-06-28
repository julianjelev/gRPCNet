using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.CashRegisters
{
    public class ArticleCategory
    {
        public ArticleCategory()
        {
            Articles = new List<Article>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string OwnerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public virtual IList<Article> Articles { get; set; }
    }
}
