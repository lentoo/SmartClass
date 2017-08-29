using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class QueryParams
    {
        /// <summary>
        /// 教室地址
        /// </summary>
        [Required]
        [StringLength(maximumLength:4,MinimumLength = 4)]
        public string classroom { get; set; }
    }
}
