### OdrabiamyDownloader
Biblioteka C# pobierająca strony i książki z Odrabiamy.pl przy użyciu ich API v2.
Biblioteka ma (nie)pełną dokumentację kodu.
# Zależności
Przy pisaniu biblioteki wykorzystałem kilka pakietów NuGet:
- System.Configuration.ConfigurationManager 6.0.0
- Newtonsoft.Json 13.0.1
- HtmlAgilityPack 1.11.42

***Próba użycia tej biblioteki bez zaimportowania tych pakietów skończy się błędem cumpilatora***
# Uwagi 
OdrabiamyDownloader domyślnie wyszukuje w pliku .config adres endpointu api, jeśli program korzystający z biblioteki
nie posiada takiego pliku należy podać adres api jako parametr konstruktora obiektu - ***w przeciwnym wypadku konstruktor zgłosi błąd***
## Użycie
Biblioteka potrafi pobierać w wersji premium i nonpremium.
# Premium
By używać wersji premium trzeba posiadać konto premium na stronie Odrabiamy.pl, z czego
istnieje limit pobrań na dzień, który podobno wynosi 60 stron (a przynajmniej tak powiedział mi mój ukochany... przyjaciel @KartoniarzEssa), 
ale w trakcie testów byłem w stanie pobrać znacznie więcej w ciągu dnia, także nie wiem jak to jest ostatecznie. 
Po napotkaniu dziennego limitu biblioteka przestaje pobierać strony i składa książkę tylko z tych stron które udało jej się pobrać.
Warto też zaznaczyć że przy pobieraniu premium należy zmienić wewnętrzne Headery obiektu OdrabiamyDownloader metodą ```ChangeHeaders()```
inaczej zostanie zgłoszony wyjątek.
# Non-Premium
Biblioteka w wersji non-premium pobiera tylko strony dostępne w wersji non-premium (czyli bardzo mało), wszystkie strony premium
będą wypłenione informacją, że dostęp do ich treści wymaga konta premium - co oznacza, że będą bezużytczene.
## Przyłady użycia biblioteki
- [Wersja Konsolowa](https://github.com/JakubCygaro/OdrabiamyDownloaderConsole)
- [Wersja Windows Forms](https://github.com/JakubCygaro/OdrabiamyDownloaderForms)
## Inspiracja
Dla mnie inspiracją była praca [KartoniarzEssa](https://github.com/KartoniarzEssa/BetterOdrabiamyDownloader), od którego ~~zerżnąłem~~ zaporzyczyłem
sposób wykorzystania API oraz gniot [Kondziorka](https://github.com/konrad11901/OdrabiamyDownloader), przez który rozbolała mnie głowa i uznałem,
że muszę sam napisać takie coś w C#. Jako iż jestem dumnym i świadomym niewolnikiem Micro$oft i Billa Gatesa, odebrałem na poziomie personalnym
jego kod i musiałem coś z tym zrobić. Postanowiłem napisać bibliotekę, a nie program tak żeby każdy mógł sobie (jeśli z jakiegoś dziwnego powodu jest mu to potrzebne)
ją wykorzystać w swoich gniotach.
