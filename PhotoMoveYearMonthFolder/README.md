﻿# 📸 Photo/Video Organizer

## 🇬🇧 English

I created this free software to solve my own need: organizing photos and videos scattered across cloud, folders, external drives, USB sticks, etc.  
Since it might be useful to others, I decided to make it available for everyone!  
This is not a commercial product: it is released under the MIT license, with no warranty.  
The program organizes your media files by year and month, automatically discarding duplicates.

✨ **Features**
- Organizes by year/month 📅
- Discards duplicates 🗑️
- Supports images & videos 🎞️
- Extracts date from Exif or filename 🕒

---

## 🔍 How duplicates and dates are detected

- **Duplicates** are found using:
  - File hash (SHA-256) 🧮
  - Exif UniqueImageID (when available) 🆔
- **Shooting date** is detected from:
  - Exif data: DateTimeOriginal, DateTimeDigitized, DateTime 🗓️
  - Filename (if it contains a date/time) 🏷️
  - Filesystem date as fallback 📁

---

**Example output:**
```
    Example:
        search in folder 1 and folder 2
		results
		folder 3
			1970  
				01
					picture1.jpg
					picture2.jpg
			2023
				01
					picture77.jpg
				02
					picture10.jpg
					picture11.jpg
				08
					picture23.jpg
					picture24.jpg
			2024
				04
					...
				05
				06
				07
				12
			2025
				01
				02
				03
				04
				05			
```
_(1970/01 is used when no valid date is found)_

---

## 📚 Libraries & Licenses

**Photo/Video Organizer** is released under the MIT License.  
See the LICENSE file for details.

This program uses third-party libraries, each with its own license:

| Library              | Version | License     | Link                                             |
|----------------------|---------|-------------|--------------------------------------------------|
| MetadataExtractor    | 2.8.1   | Apache 2.0  | https://github.com/drewnoakes/metadata-extractor |

> Please refer to each library's repository for the full license text.

---

2025 © G.L. aka Firefox_1998

---

## 🇮🇹 Italiano

Ho creato questo software gratuito per risolvere una mia esigenza: organizzare foto e video sparsi tra cloud, cartelle, dischi esterni, chiavette USB, ecc.  
Visto che può essere utile anche ad altri, ho deciso di renderlo disponibile a tutti!  
Non è un prodotto commerciale: è distribuito con licenza MIT, senza alcuna garanzia.  
Il programma organizza i file multimediali per anno e mese, scartando automaticamente i duplicati.

✨ **Funzionalità**
- Organizza per anno/mese 📅
- Scarta i duplicati 🗑️
- Supporta immagini e video 🎞️
- Estrae la data da Exif o dal nome file 🕒

---

## 🔍 Come vengono rilevati duplicati e date

- I **duplicati** vengono trovati tramite:
  - Hash del file (SHA-256) 🧮
  - UniqueImageID Exif (se disponibile) 🆔
- La **data di scatto** viene rilevata da:
  - Dati Exif: DateTimeOriginal, DateTimeDigitized, DateTime 🗓️
  - Nome file (se contiene data/ora) 🏷️
  - Data del filesystem come fallback 📁

---

**Esempio di risultato:**
```
	Example:
		search in folder 1 and folder 2
		results
		folder 3
			1970  
				01
					picture1.jpg
					picture2.jpg
			2023
				01
					picture77.jpg
				02
					picture10.jpg
					picture11.jpg
				08
					picture23.jpg
					picture24.jpg
			2024
				04
					...
				05
				06
				07
				12
			2025
				01
				02
				03
				04
				05
```
_(1970/01 viene usata quando non si trova una data valida)_

---

## 📚 Librerie & Licenze

**Photo/Video Organizer** è distribuito con licenza MIT.  
Consulta il file LICENSE per i dettagli.

Questo programma utilizza librerie di terze parti, ciascuna con la propria licenza:

| Libreria             | Versione | Licenza     | Link                                             |
|----------------------|----------|-------------|--------------------------------------------------|
| MetadataExtractor    | 2.8.1    | Apache 2.0  | https://github.com/drewnoakes/metadata-extractor |

> Consulta il repository di ogni libreria per il testo completo della licenza.

---
 
2025 © G.L. aka Firefox_1998

**Made with ❤️ by Firefox_1998**