using System.Collections;
using System.Collections.Generic;

public class CollectionSwapper<T,K> where T :  ICollection<K>, new()
{
    public T MainCollection;
    public T SecondaryCollection;

    public T SwapCollections()
    {
        T tmp = MainCollection;
        MainCollection = SecondaryCollection;
        SecondaryCollection = tmp;
        return MainCollection;
    }

    public CollectionSwapper()
    {
        MainCollection = new T();
        SecondaryCollection = new T();
    }

    public CollectionSwapper(T mainCollection, T secondaryCollection)
    {
        MainCollection = mainCollection;
        SecondaryCollection = secondaryCollection;
    }
}