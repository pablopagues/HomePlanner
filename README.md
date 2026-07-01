# HomePlanner

SaaS de administração do lar (.NET 8 + Blazor Server).

## Pendências

### Moeda dos planos para países de fala hispana ou francesa

Hoje a **tela pública de planos** (`Public/Index.razor`) mostra o preço a partir de
strings de recurso fixas por idioma (`Landing_StdPreco` / `Landing_ProPreco`):

- **pt** → `R$` (reais)
- **en** → `CA$` (dólares canadenses)
- **es** e **fr** → `CA$` (dólares canadenses) — ajustado provisoriamente; antes
  mostravam `R$`, o que estava incorreto.

Já a **tela de assinatura do tenant** (`Assinatura.razor`) calcula a moeda pelo
**país do tenant**, não pelo idioma (`AssinaturaService.MoedaDoPais`):

- `Canada` → `CAD` (`CA$`)
- qualquer outro país → `BRL` (`R$`) por padrão (fallback)

**A decidir:** como tratar de fato os valores/moeda para usuários e tenants de
países de fala hispana ou francesa (ex.: Espanha, México, França, etc.). Pontos em
aberto:

- Idioma ≠ país: es/fr no site público não implicam Canadá. Definir o mapa
  idioma → moeda vs. país → moeda de forma consistente.
- Ampliar `MoedaDoPais` / `SimboloDaMoeda` (`AssinaturaService`) e os mapas de
  preços do Stripe (`Prices`/`PricesCAD`, `ValoresBRL`/`ValoresCAD` em
  `StripeOptions`) para novas moedas, ou padronizar tudo em CAD/USD.
- Alinhar os preços da landing (resx) com os valores reais cobrados no Stripe por
  moeda, evitando divergência entre vitrine e checkout.
