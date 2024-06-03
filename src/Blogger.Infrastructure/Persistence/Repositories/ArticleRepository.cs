﻿using System.Linq;

using Blogger.Domain.ArticleAggregate.Models;

namespace Blogger.Infrastructure.Persistence.Repositories;

public class ArticleRepository(BloggerDbContext bloggerDbContext) : IArticleRepository
{
    public Task<bool> HasIdAsync(ArticleId articleId, CancellationToken cancellationToken) =>
         bloggerDbContext.Articles.AnyAsync(x => x.Id == articleId, cancellationToken);

    public void Add(Article article) =>
         bloggerDbContext.Articles.Add(article);

    public async Task<IReadOnlyCollection<ArchiveModel>> GetArchivesAsync(CancellationToken cancellationToken)
    {
        var archives = await bloggerDbContext.Articles.Where(x => x.Status == ArticleStatus.Published)
                                                 .Select(x => new
                                                 {
                                                     PublishedOnUtc = x.PublishedOnUtc!.Value,
                                                     x.Id,
                                                     x.Title,
                                                     x.PublishedOnUtc.Value.Day
                                                 })
                                                 .GroupBy(x => new { x.PublishedOnUtc.Year, x.PublishedOnUtc.Month })
                                                 .Select(x => new ArchiveModel(x.Key.Year, x.Key.Month, x.Select(z => new ArticleArchiveModel(z.Id, z.Title, z.Day))))
                                                 .ToListAsync(cancellationToken);

        return [.. archives];
    }




    public async Task<IReadOnlyList<Tag>> GetPopularTagsAsync(int size, CancellationToken cancellationToken)
    {
        var topSizeTags = bloggerDbContext.Articles
                                          .AsNoTracking()
                                          .Where(x => x.Status == ArticleStatus.Published)
                                          .SelectMany(x => x.Tags)
                                          .GroupBy(x => x.Value)
                                          .Select(x => new
                                          {
                                              Tag = x.Key,
                                              Count = x.Count()
                                          }).OrderByDescending(x => x.Count)
                                            .Take(size);

        return (await topSizeTags.ToListAsync(cancellationToken))
                                 .Select(x => Tag.Create(x.Tag))
                                 .ToImmutableList();
    }

    public async Task<IReadOnlyList<Tag>> GetTagsAsync(CancellationToken cancellationToken)
    {
        var tags = await bloggerDbContext.Articles
                                         .AsNoTracking()
                                         .Where(x => x.Status == ArticleStatus.Published)
                                         .SelectMany(x => x.Tags)
                                         .ToListAsync(cancellationToken);

        return tags.ToImmutableList();
    }




    public Task<Article?> GetDraftByIdAsync(ArticleId draftId, CancellationToken cancellationToken)
    {
        return bloggerDbContext.Articles
                               .Where(x => x.Status == ArticleStatus.Draft)
                                .FirstOrDefaultAsync(x => x.Id == draftId, cancellationToken);
    }


    public async Task<IReadOnlyList<Article>> GetPopularArticlesAsync(int size, CancellationToken cancellationToken)
    {
        var que = await bloggerDbContext.Articles.Where(x => x.Status == ArticleStatus.Published)
                                                 .OrderByDescending(x => x.CommentIds.Count)
                                                 .Take(size)
                                                 .ToListAsync(cancellationToken);

        return que.ToImmutableList();
    }

    public Task<Article?> GetArticleByIdAsync(ArticleId articleId, CancellationToken cancellationToken)
    {
        return bloggerDbContext.Articles
                                    .Where(x => x.Status == ArticleStatus.Published)
                                    .FirstOrDefaultAsync(x => x.Id == articleId, cancellationToken);
    }

    public async Task<IReadOnlyList<Article>> GetLatestArticlesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var que = await bloggerDbContext.Articles
                                        .Where(x => x.Status == ArticleStatus.Published)
                                        .OrderByDescending(x => x.PublishedOnUtc)
                                        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                                        .ToListAsync(cancellationToken);

        return que.ToImmutableList();
    }

    public async Task<IReadOnlyList<Article>> GetLatestArticlesAsync(Tag tag, CancellationToken cancellationToken)
    {
        var que = await bloggerDbContext.Articles
                                       .Where(x => x.Status == ArticleStatus.Published)
                                       .Where(x => x.Tags.Any(x => x.Value == tag.Value))
                                       .OrderByDescending(x => x.PublishedOnUtc)
                                       .ToListAsync(cancellationToken);

        return que.ToImmutableList();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await bloggerDbContext.SaveChangesAsync(cancellationToken);
    }

    public void Delete(Article draft)
    {
        bloggerDbContext.Remove(draft);
    }


}
