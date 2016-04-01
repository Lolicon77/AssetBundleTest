using UnityEngine;
using System.Collections;


public abstract class AssetBundleLoadOperation :IEnumerator
{
    public object Current
    {
        get
        {
            return null;
        }
    }

    public bool MoveNext()
    {
        return !IsDone();
    }

    public void Reset()
    {
    }
    abstract public bool Update();
    abstract public bool IsDone();
}


public abstract class AssetBundleLoadAssetOperation:AssetBundleLoadOperation
{
    public abstract T GetAsset<T>() where T : UnityEngine.Object;
}

public class AssetBundleLoadCommonAsset:AssetBundleLoadAssetOperation
{
    protected string m_AssetBundleName;
    protected string m_AssetName;
    protected string m_DownloadingError;
    protected System.Type m_Type;
    protected AssetBundleRequest m_Request = null;

    public AssetBundleLoadCommonAsset(string bundleName,string assetName,System.Type type)
    {
        this.m_AssetBundleName = bundleName;
        this.m_AssetName = assetName;
        m_Type = type;
    }

    public override T GetAsset<T>()
    {
        if(m_Request!=null && m_Request.isDone)
        {
            return m_Request.asset as T;
        }
        else
        {
            return null;
        }
    }

    public override bool Update()
    {
        if(m_Request!=null)
        {
            return false;
        }
        LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName,out m_DownloadingError);
        if(bundle!=null)
        {
            m_Request = bundle.m_AssetBundle.LoadAssetAsync(m_AssetName, m_Type);
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool IsDone()
    {
        if(m_Request==null&&m_DownloadingError!=null) 
        {
            return true;
        }
        return m_Request != null && m_Request.isDone;
    }
}

public class AssetBundleLoadManifestOperation:AssetBundleLoadCommonAsset
{
    public AssetBundleLoadManifestOperation(string bundleName,string assetName,System.Type type):
        base(bundleName,assetName,type)
    {
    }
    public override bool Update()
    {
        base.Update();
        if(m_Request!=null && m_Request.isDone)
        {
            AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
            return false;
        }
        else
        {
            return true;
        }
    }
}