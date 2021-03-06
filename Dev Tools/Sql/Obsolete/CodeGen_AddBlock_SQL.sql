/* Code Generate 'AddBlockAttributeValue(...)' for migrations. 
This will list all of the block attribute values starting with most recently Inserted
Just pick the top few that you need for your migration depending

INSERT INTO [dbo].[Block]
           ([IsSystem], [PageId], [LayoutId], [BlockTypeId], [Zone], [Order], [Name], [CssClass], [OutputCacheDuration], [Guid], [PreHtml], [PostHtml])
     VALUES

*/

begin

declare
@crlf varchar(2) = char(13) + char(10),
@BlockId int = 889

SELECT 
	'(' +
	CONVERT(nvarchar(max), b.[IsSystem]) + ', @' +
	CONVERT(nvarchar(max), REPLACE(p.InternalName,' ', '')) + 'Id, ' +
	CASE WHEN b.[LayoutId] IS NULL THEN 'NULL' ELSE '@' + CONVERT(nvarchar(max), b.[LayoutId]) + 'Id' END + ' , @' +	
	CONVERT(nvarchar(max), REPLACE(bt.[Name],' ', '')) + 'Id, ''' +
	CONVERT(nvarchar(max), b.[Zone]) + ''', ' +
	CONVERT(nvarchar(max), b.[Order]) + ', ''' +
	CONVERT(nvarchar(max), b.[Name]) + ''', ' +
	CASE WHEN b.[CssClass] IS NULL THEN 'NULL' ELSE '''' + b.[CssClass] + '''' END + ' ,' +
	CONVERT(nvarchar(max), b.[OutputCacheDuration]) + ', ''' +
	CONVERT(nvarchar(max), b.[Guid]) + ''', ' +
	CASE WHEN b.[PreHtml] IS NULL THEN 'NULL' ELSE '''' + REPLACE(b.[PostHtml], '''', '''''') + '''' END + ' ,' +
	CASE WHEN b.[PostHtml] IS NULL THEN 'NULL' ELSE '''' + REPLACE(b.[PostHtml], '''', '''''') + '''' END + ')' +
        @crlf [CodeGen Block],
		'DECLARE @'+REPLACE(bt.[Name],' ', '')+'Id int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = ''' + CONVERT(nvarchar(50), bt.Guid) + ''' )',
		'DECLARE @'+REPLACE(p.[InternalName],' ', '')+'Id int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = ''' + CONVERT(nvarchar(50), p.Guid) + ''' )'
  FROM [Block] [b]
  left join [BlockType] [bt] on [bt].[Id] = [b].BlockTypeId
  left join [Page] [p] on b.PageId = [p].[Id]
  where 
  b.Id = @BlockId

end
