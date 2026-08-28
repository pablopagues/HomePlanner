namespace Resources.HomePlanner;

/// <summary>
/// Marker class for IStringLocalizer&lt;SharedResource&gt;.
/// Strings ficam em SharedResource.resx (pt-BR padrão), SharedResource.en.resx,
/// SharedResource.es.resx e SharedResource.fr.resx.
///
/// Vive num projeto próprio porque web e app MAUI compartilham as mesmas traduções —
/// duas cópias divergiriam no primeiro texto alterado de um lado só.
/// </summary>
public sealed class SharedResource;
