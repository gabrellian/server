using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data.FileSystem;
public abstract class FileSystemRepo<TModel, TKey> where TModel : class
{
    private PropertyInfo _keyProperty;
    public string RootStoragePath { get; set; }
    public virtual string ModelStoragePath => Path.Combine(RootStoragePath, typeof(TModel).Name);
    public FileSystemRepo(IConfiguration config)
    {
        RootStoragePath = config.GetValue<string>(nameof(RootStoragePath), "./_data");
        Initialize();
    }
    protected virtual string Serialize(TModel model)
    {
        return JsonSerializer.Serialize(model);
    }
    protected virtual TModel Deserialize(string raw)
    {
        return JsonSerializer.Deserialize(raw, typeof(TModel), new JsonSerializerOptions()
        {
            WriteIndented = true
        }) as TModel;
    }

    protected virtual TKey GetNextKey() => throw new NotImplementedException();
    protected virtual string GetFileName(TKey id, TModel data) => $"{id}.json";

    private void Initialize()
    {
        Directory.CreateDirectory(RootStoragePath);
        Directory.CreateDirectory(ModelStoragePath);
    }

    public async Task<TModel> Get(TKey id)
    {
        var recordFile = Path.Combine(ModelStoragePath, id.ToString());
        if (File.Exists(recordFile))
        {
            var raw = await File.ReadAllTextAsync(recordFile);
            return Deserialize(raw);
        }
        return null;
    }

    public Task<TModel> Get(Func<TModel, bool> expression)
    {
        foreach (var file in Directory.GetFiles(ModelStoragePath))
        {
            var record = Deserialize(File.ReadAllText(file));
            if (expression(record))
            {
                return Task.FromResult(record);
            }
        }
        return Task.FromResult<TModel>(null);
    }

    public Task<IEnumerable<TModel>> Where(Func<TModel, bool> expression)
    {
        List<TModel> results = new List<TModel>();
        foreach (var file in Directory.GetFiles(ModelStoragePath))
        {
            var record = Deserialize(File.ReadAllText(file));
            if (expression(record))
            {
                results.Add(record);
            }
        }
        return Task.FromResult<IEnumerable<TModel>>(results);
    }

    public async Task<TModel> Save(TModel data)
    {
        var uniqueId = _keyProperty?.GetValue(data);

        if (uniqueId == null)
        {
            uniqueId = GetNextKey();
            _keyProperty.SetValue(data, uniqueId);
        }

        var recordFile = Path.Combine(ModelStoragePath, GetFileName((TKey)uniqueId, data));

        await File.WriteAllTextAsync(recordFile, Serialize(data));

        return data;
    }
}
