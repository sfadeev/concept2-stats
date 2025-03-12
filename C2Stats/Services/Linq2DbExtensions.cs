using LinqToDB;

namespace C2Stats.Services
{
	public static class Linq2DbExtensions
	{
		public static Task<int> MergeOnPrimaryKey<TTarget>(this ITable<TTarget> target,
			IEnumerable<TTarget> source, CancellationToken cancellationToken = default) where TTarget : notnull
		{
			return target
				.Merge()
				.Using(source)
				.OnTargetKey()
				.UpdateWhenMatched()
				.InsertWhenNotMatched()
				.MergeAsync(cancellationToken);
		}
		
		public static Task<int> MergeWithDeleteOnPrimaryKey<TTarget>(this ITable<TTarget> target,
			IEnumerable<TTarget> source, CancellationToken cancellationToken = default) where TTarget : notnull
		{
			return target
				.Merge()
				.Using(source)
				.OnTargetKey()
				.UpdateWhenMatched()
				.InsertWhenNotMatched()
				.DeleteWhenNotMatchedBySource()
				.MergeAsync(cancellationToken);
		}
	}
}