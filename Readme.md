[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/f0zg3uYt)
Un ristorante ci chiede di realizzare un gestionale per semplificare la gestione dei suoi piatti e dei suoi menù.

Ogni menù può avere più piatti, e un piatto può essere inserito in più menù. Ogni piatto deve far parte di una categoria (primo piatto, secondo piatto, contorno...), ma una categoria può contenere più piatti.

Un menù è caratterizzato da un nome.
Un piatto è caratterizzato da un nome, una descrizione e un prezzo.
Una categoria è caratterizzata da un nome.

Il cliente finale deve poter accedere a un'applicazione con cui visualizzare e manipolare tutte le informazioni a disposizione: visualizzare i menù, le categorie, i piatti, modificarli, cancellarli o aggiungerne di nuovi.

--

Dalle richieste del cliente strutturiamo la soluzione usando diversi strumenti:

1) Creiamo un database avvalendoci delle informazioni date (che tabelle creiamo? Che relazioni abbiamo fra le entità?)
2) Creiamo un back-end (ASP.Net web API) che faccia uso di tutte le nozioni apprese (dependency injection, repository pattern...) per esporre al mondo delle API di visualizzazione/modifica di tutte le informazioni richieste (menù, piatti...)
3) Creiamo un front-end (applicazione desktop WPF) che si interfacci con le API esposte dal back-end