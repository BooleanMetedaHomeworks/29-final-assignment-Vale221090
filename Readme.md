[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/f0zg3uYt)
Un ristorante ci chiede di realizzare un gestionale per semplificare la gestione dei suoi piatti e dei suoi menù.

Ogni menù può avere più piatti, e un piatto può essere inserito in più menù. Ogni piatto deve far parte di una categoria 
(primo piatto, secondo piatto, contorno...), ma una categoria può contenere più piatti.

Un menù è caratterizzato da un nome.
Un piatto è caratterizzato da un nome, una descrizione e un prezzo.
Una categoria è caratterizzata da un nome.

Il cliente finale deve poter accedere a un'applicazione con cui visualizzare e manipolare tutte le informazioni a 
disposizione: visualizzare i menù, le categorie, i piatti, modificarli, cancellarli o aggiungerne di nuovi.

--

Dalle richieste del cliente strutturiamo la soluzione usando diversi strumenti:

1) Creiamo un database avvalendoci delle informazioni date (che tabelle creiamo? Che relazioni abbiamo fra le entità?)
2) Creiamo un back-end (ASP.Net web API) che faccia uso di tutte le nozioni apprese (dependency injection, repository pattern...) 
per esporre al mondo delle API di visualizzazione/modifica di tutte le informazioni richieste (menù, piatti...)
3) Creiamo un front-end (applicazione desktop WPF) che si interfacci con le API esposte dal back-end

BONUS 1
Il cliente vuole proteggere le proprie API! Permettiamone l'accesso solo agli utenti loggati, 
fatta eccezione per le API di lettura dei piatti che deve/devono rimanere pubblica/e.

BONUS 2
Ora il cliente vuole distinguere l'utente semplice dall'utente con ruolo di amministratore! 
Limitiamo l'accesso alle API di modifica/aggiunta/cancellazione solo agli utenti con tale ruolo. 
Le API di lettura (a parte quella dei piatti, che rimane pubblica) possono continuare a essere usate dagli utenti semplici.

BONUS 3
Il cliente fa le cose in grande: sta aprendo una catena di ristoranti! Ora dobbiamo prevedere che un menù deve essere associato 
a un ristorante, mentre un ristorante può avere più menù.
Ciò significa che anche gli utenti (semplici e admin) saranno relativi a un ristorante e che quindi potranno gestire solo le 
informazioni filtrate dal loro specifico ristorante. Per questo prevediamo un nuovo ruolo, il superadmin, che ha il controllo 
completo di tutto.
Prevediamo inoltre le API relative alla lettura/scrittura di ristoranti. Solo il superadmin può manipolare i ristoranti! 
La lettura dei ristoranti, come quella dei piatti, rimane invece pubblica.

BONUS 4
Il cliente non è mai soddisfatto. Ora vuole trasparenza su cosa pensano i clienti dei suoi piatti!
Per fare ciò vorrebbe un piccolo sito web dove gli utenti possono selezionare un ristorante e visualizzarne i piatti 
(solo le informazioni base, non servono categorie e menù). Ricordiamo che queste API sono pubbliche, quindi non serve autenticazione.
Per ogni piatto verrà visualizzata la valutazione media, quanti hanno votato e la possibilità di esprimere un voto da uno a dieci.
Prevediamo allora una nuova entità, la Valutazione, che sarà 1:1 coi piatti. La valutazione conterrà il numero di voti e 
la valutazione media.
Creiamo quindi le API (pubbliche) per ottenere la valutazione di un piatto e aggiornare la valutazione con un nuovo voto. 
Assicuriamoci con opportuni controlli che la valutazione sia un numero congruo con le richieste.
Come si fa ad aggiornare una media? Supponiamo che i voti finora siano 4,8,6. La media è 18/3 = 6. Arriva un nuovo voto, 8. 
La media diventerà (6*3 + 8)/(3+1) = 26/4 = 6,5. Cioè dobbiamo:
Riprendere la vecchia media (6) e moltiplicarla per i voti avuti finora (3)
Aggiungere il nuovo voto (8)
Dividere per la nuova quantità di voti (4)