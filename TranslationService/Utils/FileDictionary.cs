using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace TranslationService.Utils;

public class FileDictionaryEventArgs<TK, TV> : EventArgs
{
    public KeyValuePair<TK, TV> Entry { get; internal set; }
}

public delegate void FileDictionaryEventHandler<TK, TV>(Object sender, FileDictionaryEventArgs<TK, TV> args);

public interface IFileDictionary<TK, TV> : IDictionary<TK, TV> {
    FileInfo File { get; }
    void Save(IDictionarySerializer<TK, TV> serializer, bool backup = false);
    void Load(IDictionarySerializer<TK, TV> serializer);

    event FileDictionaryEventHandler<TK, TV> Added;
    event FileDictionaryEventHandler<TK, TV> Removed;
    event FileDictionaryEventHandler<TK, TV> Updated;
    event EventHandler Cleared;
    event EventHandler Saved;
    event EventHandler Loaded;
}

public class FileDictionary<TK, TV>:Dictionary<TK, TV>, IFileDictionary<TK, TV>
{
    private readonly FileInfo _file;
    protected readonly SemaphoreSlim FileLock = new (1,1);
    
    public FileDictionary(FileInfo file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }
    
    public FileInfo File=>_file;

    #region IDictionaryOverrides

    public new void Add(TK key, TV value)
    {
        try
        {
            FileLock.Wait();
            base.Add(key, value);
        }
        finally{
            FileLock.Release();
        }
        OnAdded(new FileDictionaryEventArgs<TK, TV>(){Entry=new KeyValuePair<TK, TV>(key,value)});
    }

    public new void Clear()
    {
        try
        {
            base.Clear();
        }
        finally{
            FileLock.Release();
        }
        OnCleared();
    }

    public new bool ContainsKey(TK key)
    {
        try
        {
            FileLock.Wait();
            return base.ContainsKey(key);
        }
        finally
        {
            FileLock.Release();
        }
    }

    public new bool ContainsValue(TV value)
    {
        try
        {
            FileLock.Wait();
            return base.ContainsValue(value);
        }
        finally
        {
            FileLock.Release();
        }
    }

    public new int Count
    {
        get
        {
            try
            {
                FileLock.Wait();
                return base.Count;
            }
            finally
            {
                FileLock.Release();
            }
        }
    }

    public new ICollection<TK> Keys
    {
        get
        {
            try
            {
                FileLock.Wait();
                return base.Keys;
            }
            finally
            {
                FileLock.Release();
            }
        }
    }

    public new ICollection<TV> Values
    {
        get
        {
            try
            {
                FileLock.Wait();
                return base.Values;
            }
            finally
            {
                FileLock.Release();
            }
        }
    }

    public new TV this[TK key]
    {
        get
        {
            try
            {
                FileLock.Wait();
                return base[key];
            }
            finally
            {
                FileLock.Release();
            }
        }
        set
        {
            try
            {
                FileLock.Wait();
                base[key] = value;
            }
            finally
            {
                FileLock.Release();
            }
            OnUpdated(new FileDictionaryEventArgs<TK, TV>(){Entry=new KeyValuePair<TK, TV>(key,value)});
        }
    }

    public new bool Remove(TK key)
    {
        bool ret;
        try
        {
            FileLock.Wait();
            ret = base.Remove(key);
        }
        finally
        {
            FileLock.Release();
        }
        
        if(ret)OnRemoved(new FileDictionaryEventArgs<TK, TV>(){Entry=new KeyValuePair<TK, TV>(key,default)});

        return ret;
    }

    #endregion

    #region Events
    
    public event FileDictionaryEventHandler<TK, TV> Added;
    public event FileDictionaryEventHandler<TK, TV> Removed;
    public event FileDictionaryEventHandler<TK, TV> Updated;
    public event EventHandler Cleared;
    public event EventHandler Saved;
    public event EventHandler Loaded;
    
    protected void OnAdded(FileDictionaryEventArgs<TK, TV> args)
    {
        Added?.Invoke(this, args);
    }

    protected void OnRemoved(FileDictionaryEventArgs<TK, TV> args)
    {
        Removed?.Invoke(this, args);
    }

    protected void OnUpdated(FileDictionaryEventArgs<TK, TV> args)
    {
        Updated?.Invoke(this, args);
    }
    
    protected void OnCleared()
    {
        Cleared?.Invoke(this, EventArgs.Empty);
    }

    protected void OnSaved()
    {
        Saved?.Invoke(this, EventArgs.Empty);
    }
    
    protected void OnLoaded()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
    }
    
    #endregion
    
    public void Save(IDictionarySerializer<TK, TV> serializer, bool backup = false)
    {
        try
        {
            FileLock.Wait();

            File.Refresh();
            if (File.Exists && backup)
                File.MoveTo(File.Directory?.FullName + Path.DirectorySeparatorChar +
                            $"_backup{DateTime.Now:yyyyMMddHHmmssfff}_" + File.Name);

            using Stream s = new FileStream(File.FullName, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new StreamWriter(s, Encoding.UTF8);
            serializer.Serialize(this, sw);
        }
        finally
        {
            FileLock.Release();
            OnSaved();
        }
    }
    
    public void Load(IDictionarySerializer<TK, TV> serializer)
    {
        try
        {
            File.Refresh();
            if (!File.Exists) return;

            using Stream s = new FileStream(File.FullName, FileMode.Open, FileAccess.Read);
            using StreamReader sr = new StreamReader(s, Encoding.UTF8);
            Clear();
            serializer.Deserialize(this, sr);
        }
        finally
        {
            FileLock.Release();
            OnLoaded();
        }
    }
}