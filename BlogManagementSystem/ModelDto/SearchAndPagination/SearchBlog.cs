﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ModelDto.SearchAndPagination
{
    public class SearchBlog
    {
        public string? Title { get; set; }
        public string? Content { get; set; }

        public string? Username { get; set; }
        public DateTime? BeforeDate { get; set; }
        public DateTime? AfterDate { get; set; }

        [DefaultValue(1)]
        [Range(1, 250)]
        public int Page { get; set; } = 1;
        [DefaultValue(5)]
        [Range(5, 250)]
        public int PageSize { get; set; } = 5;
    }
}
