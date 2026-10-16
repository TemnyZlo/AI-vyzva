## Nápad
- Název projektu: Nachytej se nanečisto (pracovní)
- Oblast výzvy: Prevence podvodů
- Cílová skupina: lidé 60+ a jejich dospělé děti, které jim nástroj ukážou
- Problém: 77,5 % se obává podvodů s AI; rozpoznání deepfaku si věří jen 26,4 % (u neuživatelů AI 13,2 %); 60+ nejvyšší obava (60,2 %)
- Řešení v jedné větě: rodinný trenažér, kde senior s pomocí dítěte trénuje na realistických neškodných podvodech šitých na míru a odnáší si rodinná pravidla
- Role AI: personalizace scénářů a vysvětlení; vyhodnocení podle pevných šablon, ne AI

## Rozsah MVP
- Must-have: dotazník dvojice, vstupní kolo 6 zpráv (2 legitimní), volba "co uděláš", AI vysvětlení se zvýrazněním, tipy pro mladšího, výstupní kolo + skóre před/po, kartička s rodinnými pravidly
- Nice-to-have: hlas, sdílení, anonymní statistiky
- Vědomě vynecháno: detektor skutečných zpráv, účty, ukládání osobních údajů

## Technické volby
- Backend: .NET (ASP.NET Core Minimal API), SQLite
- Hosting / deploy: Azure App Service F1 (zdarma), rate limiting kvůli API klíči

## Log rozhodnutí
- 16. 10.: MVP: text, personalizace, režim "mladší ukazuje staršímu"

- Dotazník: režim "sám" i "ve dvojici" (panel pro průvodce jen ve dvojici)
- Zlatá pravidla: pevný ověřený text (ne AI) – banka nechce PIN/kódy; zavěsit a volat sám na číslo z karty; žádný "bezpečný účet"; nic neinstalovat; "nové číslo" ověřit; spěch = varování; co dělat, když se to stalo (banka, 158)
- Pravidla se zobrazují: u vysvětlení scénáře, v souhrnu, na kartičce

## Log rozhodnutí
- 16. 10.: Přidán režim pro jednotlivce a pevná zlatá pravidla (ne generovaná AI)


## Technické volby
- AI model / API: obsah (scénáře, varianty, vysvětlení) předgenerován AI offline a ručně zkontrolován; aplikace vybírá dle profilu – bez runtime nákladů
- Volitelně: živá personalizace přes free tier LLM s fallbackem na předgenerovaný obsah

## Log rozhodnutí
- 16. 10.: Bez placeného LLM za běhu. Argument pro porotu: "AI tvoří, člověk ověřuje, senior nevidí neověřený výstup AI"
- Riziko: kritérium "smysluplnost AI" – zmírnit popisem procesu + případně živou vrstvou

## Limity, chyby a data
- Hlavní rizika: chybný obsah; přehnaná podezíravost; falešný pocit bezpečí; zastarání; stud seniora; zneužití knihovny
- Ošetření: pevné správné odpovědi v šablonách + lidská kontrola; 1/3 legitimních zpráv, "ověř" místo "ignoruj"; explicitně "trénink nezaručuje bezpečí"; datum a zdroj u scénářů; laskavý tón, skóre až na konci; fiktivní značky a neexistující kontakty, jen veřejně známé vzory
- Data: žádné osobní údaje ani účty; anonymní statistika jen se souhlasem; IP neukládáme; hosting EU; stránka "Jak to funguje a co nevíme"

## Technické volby
- Frontend+backend: ASP.NET Core Razor Pages, EF Core + SQLite
- Obsah: Content/scenarios.json + rules.json (verzované)
- Výběr: filtr dle profilu + párové scénáře stejné obtížnosti pro férové před/po
- 5 univerzálních akcí, bodování per scénář

## Log rozhodnutí
- 16. 10.: Limity a architektura schváleny (Razor Pages, předgenerovaný obsah, párové scénáře)

## Rozsah MVP (doplněno 16. 10.)
- Personalizace bez AI: oslovení, jméno blízkého (+ rod), banka; dosazení do šablon zpráv; jen v session, mimo statistiky

## Log rozhodnutí
- 16. 10.: Personalizace řešena šablonami, ne AI (rozhodnutí autora)