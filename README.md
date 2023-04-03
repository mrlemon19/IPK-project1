# IPK Projekt 1: IPK Calculator Protocol
## autor: Jakub Lukáš
## login: xlukas18

## Základní popis a funkcionalita
Cílem projektu bylo vytvořit webový klient pro vzdálenou kalkulačku na serveru. Aplikace operuje pomocí UDP (binární varianta), nebo TCP (textová varianta) protokolu. Posílá výrazy k vypočítání a server posílá zpět výsledky. Pro řešení projektu jsem vybral jazyk C\#. Usage: "./ipkcpc -h <host> -p <port> -m <mode>". Kde host je IP adresa serveru, se kterým aplikace bude komunikovat. Port je číslo portu, na kterém bude aplikace fungovat. A mode je výběr komunikačního protokolu, UDP, nebo TCP.
### Binární varianta
Tato varianta se serverem komunikuje pomocí UDP protokolu. Neprobíhá zde žádné propojení mezi serverem a klientem, klient pouze posílá zprávy v korektním formátu a servr na ně odpovídá v korektním formátu. Klientem odesílaná zpráva se zkládá z osmi bitů opcode (0/1, u zprávy odesílané klientem 0), osmi bitů délky odesílané zprávy a samotné zprávy zadané uživatelem. Výraz posílaný ve zprávě má postfixový tvar a je ohraničen závorkam. Skládá se z operátorů (+, -, *, /) a čísel. Přijímaná zpráva obsahuje osm bitů opcode (0/1, tady 1), osm bitů status code (0 - ok, 1 - error), osm bitů délky zprávy a samotná zpráva. Když je vše v pořádku, vypíše se OK: "Výsledek". Když nastane chyba, vypíše se ERR: "chybová hláška".
### Textová varianta
V této variantě se využívá TCP protokol. To znamená, že klient se serverem naváže spojení a až po té je možná komunikace. Po spuštění je potřeba komunikaci započít počáteční zprávou "HELLO", když server pozdrav opětuje je připravený ke komunikaci. Server pak přijímá zprávy s požadavkem na výpočet ve tvaru SOLVE ("infixový výraz"). Když je vše v pořádku, odpoví zprávou RESULT "Výsledek". Komunikace se serverem se ukončí zprávou BYE, kdy server odpový stejně a komunikaci ukončí. Při přerušujícím signálu (C-c) je komunikace korektně ukončena.
## Implementace
Hlavní funkcionalita je rozdělená do funkcí connect_tcp a connect_udp. Ve funkci main jsou pouze zpracovány parametry a podle zvoleného módu je zavolána příslušná funkce.
### Binární varianta
Jako první se vytvoří socket pro udp komunikaci a IPEndPoint pro zadanou IP adresu a port. Dále se ve while smyčce pokaždé načte řádek ze standardního vstupu a přidá se před něj opcode a délka zprávy, obě reprezentovány jako ascii znak. Celá zpráva se zakóduje a pošle na IPEndPoint serveru. Z IPEndPoint se příjme odpověď serveru a vytiskne se.
### Textová varianta
Začne se inicializováním TCP secketu a vytvořením IPEndPoint pro server na zadané IP adrese a portu. Socket je pak napojen na IPEndPoint. Je vytvořený tcp stream a jsou incializovány StreamWriter TCPWriter a StreamReader TCPReader. Ze standartního vstupu jsou pak po řádcích načítány zprávy a pomocí TCPWriter sou po streamu posílány serveru. Odpověď serveru zachytí TCPReader a ta je pak vypsána. Jakmile je od serveru obdržena zpráva BYE, komunikace je ukončena. Pro případ, že uživatel Přeruší chod aplikace signálem C-c je tu ConsoleCancelEventHandler OnExitTCP. Ten při signálu C-c zavolá funkci CloseTCP, která pošle serveru zprávu BYE a ukončí tím korektně komunikaci. Následně zavře všechny prostředky používané ke komunikaci (TCPWriter, TCPReader, TCPSocket). Ty jsou globálně definované mimo funkci, aby je bylo možné ukončit při signálu C-c.
## Funkčnost nad rámec zadání
Jedna věc lehce vybočující zadání je, že není dbáno na pořadí argumentů při spouštění programu. Nic méně musí být všechny parametry zadány aby program fungoval správně.
## Testování
Projekt byl testován v dodaném vývojovém prostředí. Všechny testy proběhly podle plánu. Vstupy a výsledky testů jsou ve složce tests.
## Bibliografie
- Kurose J.F., Ross K.W.: Computer Networking, A Top-Down Approach Featuring the Internet (8th edition). Addison-Wesley, 2021.
- Andrew S. Tanenbaum,Nick Feamster, David J. Wetherall. Computer Networks, 6th Edition, Pearson, 2021.
