using System;

namespace XDiscordBotLib.Utils;

public static class Singleton<T> where T : class
{
    private static readonly object m_instanceLock = new object();

    private static bool m_sealed;
    private static T m_instance;

    public static T Instance => m_instance;

    public static bool IsSealed => m_sealed;

    public static T SetInstance(T instance, bool seal = true)
    {
        if (m_sealed)
            throw new InvalidOperationException("Singleton is sealed. Can't set instance");

        m_instance = instance;
        m_sealed = seal;
        return instance;
    }

    public static void Seal() { m_sealed = true; }
}