# Ochrana dat a GDPR – Nachytej se nanečisto

Stav k 16. 10. 2026. Popis odpovídá kódu v `src/Nachytej`.

## Princip
Nepotřebujeme vědět, kdo uživatel je. Žádné účty, žádné kontaktní údaje, žádné měřicí ani reklamní nástroje, žádný obsah z cizích serverů.

## Co se kam ukládá

| Údaj | Kde | Doba | Účel | Kde v kódu |
|---|---|---|---|---|
| Technická cookie `.Nachytej.Session` (náhodný identifikátor, HttpOnly) | prohlížeč | do zavření prohlížeče | navázání na rozpracovaný trénink | `Program.cs` (AddSession) |
| Antiforgery cookie | prohlížeč | do zavření prohlížeče | ochrana formulářů | ASP.NET Core (výchozí) |
| Průběh tréninku: odpovědi ano/ne, vybrané odpovědi, skóre | paměť serveru (`DistributedMemoryCache`), nic na disku | max. 2 h od poslední akce, restart serveru, nebo tlačítko „Smazat“ | fungování tréninku | `Services/TrainingSession.cs`, `Models/Training.cs` |
| Nepovinná personalizace: oslovení, jméno blízkého (+ rod), název banky | paměť serveru, spolu s průběhem | jako výše | dosazení do cvičných zpráv | `Services/Personalizer.cs` |
| Anonymní výsledek – **jen se souhlasem** | soubor `data/stats.jsonl` na serveru | nejdéle 2 roky | ověření přínosu (před/po) | `Services/StatsStore.cs` |

### Obsah anonymního výsledku
- náhodné GUID tréninku (není spojené s člověkem ani se zařízením po skončení session),
- čas dokončení, režim (sám / ve dvojici),
- 5 odpovědí ano/ne z dotazníku,
- pro každou zprávu: ID scénáře, kategorie zvolené akce, body.

**Neobsahuje:** jméno, kontakt, IP adresu, user-agent, ani údaje z personalizace. Odstranění personalizace zajišťuje `Profile.Anonymized()`. Ověřeno testem: 14 tréninků s vyplněným jménem a bankou, v souboru 0 výskytů.

## Co aplikace nedělá
- neukládá IP adresy (logování ASP.NET Core je na úrovni Warning, bez záznamu požadavků),
- nepoužívá analytiku, reklamní cookies, externí písma ani skripty → není potřeba cookie lišta,
- nezpracovává skutečné zprávy uživatele (není to detektor; uživatel nic nevkládá),
- nemá účty ani hesla.

## Právní základ (čl. 6 GDPR)
- průběh tréninku a personalizace: plnění služby, o kterou uživatel žádá (písm. b),
- technické cookies: nezbytné pro fungování, bez souhlasu,
- anonymní výsledek: souhlas (písm. a), pokud by byl považován za osobní údaj.

## Poctivě uvedené limity
- Anonymní výsledek nelze na žádost dohledat a smazat – nevíme, který komu patří. Proto je ukládání dobrovolné a uživatel je o tom předem informován.
- Uživatel může do personalizace napsat jméno třetí osoby (dítěte). Minimalizace: stačí křestní jméno, drží se jen v paměti po dobu tréninku, do statistik se nedostane.
- Poskytovatel hostingu (Microsoft Azure) může zpracovávat technické údaje o spojení podle svých podmínek. Region: EU.

## Pro uživatele
Stránka **/OchranaDat** v aplikaci: tabulka výše srozumitelně, práva subjektu údajů, kontakt na správce, odkaz na ÚOOÚ a tlačítko „Smazat průběh mého tréninku“.

## Před zveřejněním doplnit
V `appsettings.json` → `Privacy:Controller` (jméno správce) a `Privacy:Contact` (kontaktní e-mail). Dokud nejsou vyplněné, stránka zobrazuje výrazné „[DOPLNIT: …]“.
