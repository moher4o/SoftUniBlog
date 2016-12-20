﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class Article
    {
        private ICollection<Tag> tags;
        private ICollection<Comment> comments;
        private ICollection<ArticleImage> images;

        public Article()
        {
            this.Tags = new HashSet<Tag>();
            this.comments = new HashSet<Comment>();
            this.images = new HashSet<ArticleImage>();

        }

        public Article(string authorId, string title, string content, int categoryId)
        {
            this.AuthorId = authorId;
            this.Title = title;
            this.Content = content;
            this.CategoryId = categoryId;
            this.VisitCounter = 0;
            this.Tags = new HashSet<Tag>();
            this.comments = new HashSet<Comment>();
            this.images = new HashSet<ArticleImage>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public string Content { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public int VisitCounter { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<ArticleImage> Images
        {
            get { return this.images; }
            set { this.images = value; }
        }

        public virtual ICollection<Comment> Comments
        {
            get { return this.comments; }
            set { this.comments = value; }
        }

        public virtual ICollection<Tag> Tags
        {
            get { return this.tags; }
            set { this.tags = value; }
        }

        public bool IsUserAuthor(string username)
        {
            return this.Author.UserName.Equals(username);
        }
    }
}