using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces.Parsers
{
    public interface IParser<TEntity, TModel>
    {
        IEnumerable<TModel> Parse(IEnumerable<TEntity> entities);
    }
}
