using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Manager
{
    public class Nested
    {
        private static Dictionary<String, Type> _entitiesList = new Dictionary<String, Type>();

        public static void AddEntities<T>() where T : class, new()
        {
            var entityType = typeof(T);

            if (!_entitiesList.ContainsKey(entityType.Name))
                _entitiesList.Add(entityType.Name, entityType);
        }

        public T FillEntities<T>(NestedLine nestedProperties) where T : class, new()
        {
            var mainEntityName = nestedProperties.LevelName(1);

            if (!_entitiesList.ContainsKey(mainEntityName)) return null;

            var propertyEntityType = _entitiesList[mainEntityName];

            var propEntity = Activator.CreateInstance(propertyEntityType);

            return (T)Match(nestedProperties, propEntity, propertyEntityType, 2);
        }

        public NestedEntities BuildNestedEntities(NestedLines nestedLines, String entityName, NestedEntity parent, int level = 1)
        {
            var nestedEntities = new NestedEntities();

            foreach (var line in nestedLines.Lines)
            {
                var idField = line.GetId(entityName, level);

                if (idField == null || !_entitiesList.ContainsKey(entityName)) continue;

                var entityType = _entitiesList[entityName];

                NestedEntity nestedEntity = new NestedEntity(idField.Value.ToString(), entityName, entityType);

                nestedEntity.NestedFields.AddRange(line.Fields.Where(x => x.Name.Length == level + 1).ToList());

                if (parent != null) parent.AddChild(nestedEntity.EntityName, nestedEntity);

                var fieldsByEntity = line.GroupFieldsByEntity(level + 1);

                foreach (var item in fieldsByEntity)
                {
                    if (!_entitiesList.ContainsKey(item.Key)) continue;

                    BuildNestedEntities(new NestedLines(new List<NestedLine> { new NestedLine(item.ToList()) }), item.Key, nestedEntity, level + 1);
                }

                nestedEntities.Add(nestedEntity);
            }

            return nestedEntities;
        }

        public List<object> BuildEntities(NestedEntities entities)
        {
            List<object> returnEntities = new List<object>();

            foreach (var nestedEntity in entities.Entities)
            {
                var entity = Activator.CreateInstance(nestedEntity.EntityType);

                var properties = nestedEntity.EntityType.GetProperties();

                foreach (var field in nestedEntity.NestedFields)
                {
                    var currentProp = properties.FirstOrDefault(x => x.Name.Equals(field.PropertyName(), StringComparison.InvariantCultureIgnoreCase));

                    if (currentProp == null) continue;

                    if (currentProp.PropertyType.GUID != field.Type.GUID) continue;

                    currentProp.SetValue(entity, field.Value);
                }

                foreach (var child in nestedEntity.Childs)
                {
                    var childProperty = properties.FirstOrDefault(x => x.Name.Equals(child.Key, StringComparison.InvariantCultureIgnoreCase));

                    if (childProperty is null) continue;

                    if (childProperty.PropertyType.GUID != child.Value.EntityType.GUID) continue;

                    childProperty.SetValue(entity, BuildEntities(new NestedEntities().Add(child.Value)).FirstOrDefault());
                }

                returnEntities.Add((object)entity);
            }

            return returnEntities;
        }

        private object Match(NestedLine nestedFields, object entity, Type entityType, int level)
        {
            var properties = entityType.GetProperties();

            var entityName = entityType.Name;

            var fields = nestedFields.FieldsByLevel(level);

            var id = nestedFields.GetId(entityName, level);

            //NestedEntity nestedEntity = new NestedEntity(id, entityName, entityType);

            //var nestedEntities = new NestedEntities();
            //nestedEntities.Add(nestedEntity);

            foreach (var prop in properties)
            {
                var currentField = fields.FirstOrDefault(x => x.Name[level].Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase));

                if (currentField == null) continue;

                if (currentField.Type.GUID != prop.PropertyType.GUID) continue;

                prop.SetValue(entity, currentField.Value);
            }

            var classProperties = properties.Where(x => x.PropertyType.IsClass);

            foreach (var prop in classProperties)
            {
                if (!_entitiesList.ContainsKey(prop.Name)) continue;

                var propertyEntityType = _entitiesList[prop.Name];

                var propEntity = Activator.CreateInstance(propertyEntityType);

                var propValue = Match(nestedFields, propEntity, propertyEntityType, level + 1);

                prop.SetValue(entity, propValue);
            }

            return entity;
        }
    }


    public class NestedLine
    {
        private readonly List<NestedField> _fields;

        public List<NestedField> Fields => _fields;

        public NestedLine(List<NestedField> fields)
        {
            _fields = fields;
        }

        public static NestedLine FillFields(DbDataReader reader)
        {
            List<NestedField> _fields = new List<NestedField>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var currentField = new NestedField(
                    reader.GetName(i),
                    reader.GetFieldType(i),
                    reader.GetValue(i));

                _fields.Add(currentField);
            }

            return new NestedLine(_fields);
        }

        public NestedField GetId(String entityName, int level)
        {
            return Fields.Find(x => x.Name[level] == "Id" && x.Name[level - 1] == entityName);
        }


        /// <summary>
        /// Start from 1
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<NestedField> FieldsByLevel(int level)
        {
            return _fields
                 .Where(x => x.Name.Length <= level)
                .ToList();
        }

        public String LevelName(int level)
        {
            return _fields
                .Where(x => x.Name.Length >= level)
                .Select(x => x.Name[level - 1])
                .First();
        }

        public IEnumerable<IGrouping<String, NestedField>> GroupFieldsByEntity(int level)
        {
            return _fields.GroupBy(x => x.Name[level - 1]);
        }
    }

    public class NestedQuery
    {
        public NestedLines BuildEntities(DbDataReader reader)
        {
            List<NestedLine> lines = new List<NestedLine>();
            while (reader.Read())
            {
                var line = NestedLine.FillFields(reader);
                lines.Add(line);
            }
            return new NestedLines(lines);
        }
    }

    public class NestedLines
    {
        private readonly List<NestedLine> _lines;

        public NestedLines(List<NestedLine> lines)
        {
            _lines = lines;
        }

        public List<NestedLine> Lines => _lines;

        public NestedLines FilterByEntityName(String entityName, int level = 1)
        {
            return new NestedLines(_lines.Select(x => new NestedLine(x.FieldsByLevel(level)))
                .Where(x => x.LevelName(level) == entityName)
                .ToList());
        }

        public NestedField GetId(String entityName, int level, NestedLine line)
        {
            return line.Fields.Find(x => x.Name[level] == "Id" && x.Name[level - 1] == entityName);
        }

        public IEnumerable<IGrouping<object, NestedLine>> GroupById(int level = 1)
        {
            return _lines.Select(x => new NestedLine(x.FieldsByLevel(level)))
               .GroupBy(x => GetId("Id", level, x)?.Value);
        }

        public IEnumerable<IGrouping<object, NestedLine>> GroupByEntityName(int level)
        {
            return _lines.Select(x => new NestedLine(x.FieldsByLevel(level)))
               .GroupBy(x => GetId("Id", level, x)?.Value);
        }
    }

    public class NestedEntities
    {
        //public Dictionary<Tuple<String, Type>, NestedEntity> _entitiesByIdAndType = new Dictionary<Tuple<String, Type>, NestedEntity>();

        List<NestedEntity> _entities = new List<NestedEntity>();

        public List<NestedEntity> Entities => _entities;

        public NestedEntities Add(NestedEntity entity)
        {
            _entities.Add(entity);

            return this;
        }

        //public NestedEntities Add(NestedEntity entity)
        //{
        //    var currentEntityTuple = new Tuple<string, Type>(entity.EntityName, entity.EntityType);
        //    if (_entitiesByIdAndType.ContainsKey(currentEntityTuple))
        //    {
        //        _entitiesByIdAndType[currentEntityTuple].NestedLines.AddRange(entity.NestedLines);
        //    }
        //    else
        //    {
        //        _entitiesByIdAndType.Add(currentEntityTuple, entity);
        //    }

        //    return this;
        //}
    }

    public class NestedEntity
    {
        public NestedEntity(String id, string entityName, Type entityType)
        {
            Id = id;
            EntityName = entityName;
            EntityType = entityType;
            Childs = new Dictionary<String, NestedEntity>();
            NestedFields = new List<NestedField>();
        }

        public String Id { get; set; }

        public String EntityName { get; set; }

        public Type EntityType { get; set; }

        public List<NestedField> NestedFields { get; set; }

        public Dictionary<String, NestedEntity> Childs { get; set; }

        internal void AddChild(string entityName, NestedEntity nestedEntity)
        {
            Childs.Add(entityName, nestedEntity);
        }

        internal void BuildEntity()
        {

        }
    }

    public class NestedField
    {
        public NestedField(string name, Type type, object value)
        {
            Name = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            Type = type;
            Value = value;
        }

        public String[] Name { get; set; }

        public Type Type { get; set; }

        public object Value { get; set; }


        public String PropertyName()
        {
            return Name[Name.Length - 1];
        }
    }
}
