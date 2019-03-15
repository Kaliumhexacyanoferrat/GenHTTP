using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using GenHTTP.Abstraction;

namespace Performance {
  
  public class ReplaceTest {

    public void Run() {
      string testString = @"Was muss ich bei einer Neuinstallation beachten?
Allgemeine Checkliste vor einer Neuinstallation

Betrifft FAQ

Früher oder später ist bei jedem Computer eine Neuinstallation fällig. Sei es nun wegen einem Viren/Spywarebefall, weil eine neuere Betriebssystemversion installiert werden soll oder weil das installierte Betriebssystem mit der Zeit einfach zu langsam geworden ist. Dieser Artikel soll Ihnen helfen sich einen Überblick darüber zu schaffen, was für eine saubere Neuinstallation nötig ist und wie Sie dazu am Besten vorgehen.

Zunächst einmal ist es wichtig sich etwas Zeit zu nehmen, um das neue System zu planen. Während der Neuinstallation können Sie das Betriebssystem viel besser an Ihre Bedürfnisse anpassen als dies später der Fall sein wird.

Diese Anleitung wird im Folgenden auf wichtige Punkte vor der Neuinstallation eingehen und weiterführend auch die Installation von Treibern und Updates etc. behandeln.

1. Datensicherung

Bei einer Neuinstallation gehen auf der Festplatte (zumindest auf der Systempartition) alle Daten verloren. Nichts ist ärgerlicher als festzustellen, dass Sie beispielsweise Ihre Familienfotos nicht gesichert haben. Gehen Sie daher die Festplatte Ordner für Ordner durch und vergewissern Sie sich ruhig mehrmals, dass Sie alle Daten gespeichert haben. Kopieren Sie die Daten auf Speichermedien wie externe Festplatten, USB-Sticks, DVDs oder andere Datenträger um diese nach der Neuinstallation wiederherstellen zu können. Haben Sie Ihre Festplatte in mehrere Partitionen aufgeteilt, können Sie Ihre Daten auch auf eine Partition sichern, die nicht von der Formatierung betroffen ist.
Denken Sie bei der Datensicherung auch an Daten wie die Favoriten Ihres Browsers, Adressbücher oder Ihre E-Mails.

2. Systemeinstellungen

Im Normalfall haben Sie an Ihrem System einige Dinge angepasst. Speichern Sie daher später hinzugefügte Registrierungseinstellungen und modifizierte Systemdateien (z.B. uxtheme.dll) ebenfalls auf einem externen Datenträger ab. Auch an die Konfiguration der Dienste sollten Sie denken (HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services). Die Einstellungen von Programmen sollten Sie oftmals auch ex- bzw. später wieder importieren können.

3. Treiber

Damit das Betriebssystem den Computer verwalten kann benötigt es Treiber. Im allgemeinen veröffentlichen die Hersteller von Computerteilen ab und zu einen neuen Treiber, der Probleme beheben oder mehr Leistung bieten soll. Daher empfiehlt es sich bei einer Neuinstallation die neusten Treiber zu installieren. Sinnvoll ist es diese Treiber bereits vor der Neuinstallation herunterzuladen da Sie dies später aufgrund des fehlenden Netzwerkkarten/Modemtreibers nicht mehr nachholen können. Um herauszufinden welche Geräte in Ihrem Computer arbeiten können Sie das Programm Everest Home Edition (Download bei ZDNet) verwenden, dass Ihnen auch Links zu den Hersteller- und Treiberwebseiten angibt. Die wichtigsten Treiber sind zunächst einmal der Chipsatztreiber, der Grafikkartentreiber, der Netzwerkkartentreiber und der Treiber für die Tastatur und die Maus. Treiber für die Soundkarte, die Digitalkammera oder den Drucker können auch erst nach der Neuinstallation heruntergeladen werden.

Wenn Sie Windows auf eine S-ATA-Festplatte installieren möchten benötigen Sie dafür spezielle Treiber, die über eine Diskette während des Installationsvorgangs eingelesen werden. Leider bieten die Installationsroutinen von Windows 2000 und Windows XP keine Möglichkeit den Treiber zum Beispiel über das CD-ROM-Laufwerk einzulesen. Die meisten Chipsatzhersteller bieten ein Programm an, mit dem es möglich ist die Treiberdiskette zu erstellen.

4. Tools und Programme

Vor der Neuinstallation ist es sinnvoll die Installationsdateien für Programme und Tools die Sie benötigen herunterzuladen und auf einen externen Datenträger zu sichern. Wichtig sind hier insbesondere der Virenscanner und die Firewall, die Sie schnellmöglichst nach der Neuinstallation einrichten sollten.

5. Das Installationsmedium

Mit der Anpassung der Installations-CD können Sie sich einiges an Zeit sparen. So ist es möglich das SP2 für Windows XP in die Installations-CD zu integrieren und die CD mit NLite an Ihre Bedürfnisse anzupassen. So sparen Sie sich die Installation des ServicePacks und die Deinstallation von nicht benötigten Komponenten.

6. Partitionierung

Vor der eigentlichen Neuinstallation sollten Sie sich Gedanken zum Aufteilen Ihrer Festplatte in sogenannte Partitionen machen. Ist Ihre Festplatte bereits in Partitionen aufgeteilt, können Sie diesen Schritt überspringen. Der Vorteil von Partitionen liegt im Trennen der Systemdaten von den Benutzerdaten. Wenn Sie Ihre Festplatte in zwei Partitionen aufteilen, können Sie auf den einen Teil Windows installieren und auf den anderen Ihre Daten speichern. Bei einer erneuten Neuinstallation können Sie die Daten auf der Datenpartition belassen und müssen nur die Systempartition formatieren. Dadurch fällt ein Großteil der lästigen Datensicherung weg.

Es gibt keine perfekte Partitionierung, weil die Bedürfnisse an die Aufteilung der Festplatte bei jedem Benutzer verschieden sind. Zu beachten gilt allerdings:
Wenn Sie ein älteres Betriebssystem neben Windows 2000/XP installieren möchten, müssen Sie zuerst Windows 95/98/ME auf die primäre Partition (im Normalfall C:, etwa 5 GB sollten reichen) und Windows 2000 oder XP auf eine erweiterte Partition installieren (z.B. D:, circa 10-15 GB ohne Programme).
Windows 95/98/ME kann nur FAT32-Partitionen erkennen. D.h. wenn Sie Daten zwischen den Betriebssystemen austauschen möchten muss dies über eine FAT32-Partition geschehen
Auch Linux kann im Normalfall nicht auf NTFS-Partitionen schreiben.
NTFS unterstützt im Gegensatz zu FAT32 Datenverschlüsselung und ein Rechtesystem.

Die Partitionierung geschieht bei Windows 2000 und XP über die Setuproutine. Möchten Sie allerdings einen Dualboot mit Windows 95/98/ME einrichten, müssen Sie z.B. mit der MS-DOS-Startdiskette (mit FDISK) eine primäre Partition erstellen und auf diese Windows 95/98/ME installieren.

7. Installation

Nun sind alle Vorbereitungen abgeschlossen und Sie können Windows auf Ihre Festplatte installieren. Wenn Sie ein älteres Betriebssystem zusätzlich zu Windows 2000/XP installieren möchten, müssen Sie dieses als erstes Installieren. Windows 2000/XP richtet dann einen Bootmanager ein, mit dem Sie beim Start des Computers das jeweilige Betriebssystem auswählen können.

Installationsanleitungen:

Windows XP
Windows 2000

8. Images

Nach der Installation ist es möglich, den gesamten Inhalt der Systempartition zu sichern, um ihn bei Bedarf wiederherstellen zu können (z.B. mit dem Programm True Image). Zu welchem Zeitpunkt Sie ein Image erstellen bleibt Ihnen überlassen. Wenn Sie ein Image erstellen, wenn Sie bereits die Programme und Treiber installiert haben, haben Sie den Nachteil, dass neue Treiber- und Programmversionen umständlich ersetzt werden müssen. Andrerseits müssen Sie bei einem ""nackten"" Image die Treiber und Programme allesamt neu installieren.

9. Treiber

Bei der Treiberinstallation empfiehlt es sich, zunächst die heruntergeladenen Chipsatztreiber zu installieren und dann den Computer neu zu starten. Nachdem Sie die Grafikkartentreiber installiert und wiederrum neu gestartet haben, können Sie die Treiber für Netzwerkkarte, Soundkarte etc. auf einmal installieren. Jetzt können Sie die Bildschirmauflösung einstellen und die Soundkonfiguration anpassen. Vor der Konfiguration der Netzwerkkarte/des Modems empfiehlt es sich die Firewall und den Virenscanner zu installieren.

10. Windows-Updates

Nachdem Sie alle Treiber installiert haben sollten Sie Ihren Computer mit den Windows-Updates auf den neusten Stand bringen. Über die Windows-Update-Seite können Sie die benötigten Sicherheits- und Programmupdates einfach und automatisch installieren lassen.

11. Benutzerkonfiguration

Wenn der Computer für mehrere Benutzer konfiguriert werden soll, können Sie jetzt die Benutzer anlegen und deren Rechte festlegen. Im allgemeinen sollte niemand mit Administratorrechten am Computer arbeiten, ein Konto mit Administratorrechten sollte allerdings für Installationen und Treiberaktuallisierungen vorhanden sein. Dies ist sicherer als wenn Sie beim Arbeiten mit dem Computer ständig über die vollen Administrationsrechte verfügen. Vergessen Sie nicht unter Windows XP Home, den Benutzer ""Administrator"" mit einem Kennwort zu versehen, weil sonst jeder über den abgesicherten Modus Vollzugriff auf Ihr System hat.

12. Programme und Daten

Jetzt können Sie abschließend die Daten von den externen Datenträgern wiederherstellen und die benötigten Programme installieren und einrichten. Achten Sie darauf, dass Sie die Programme und Daten auf die Datenpartition speichern, damit die Systempartition nicht voll wird und die Trennung von System und Daten gewährleistet ist. Vergessen Sie auch nicht, die Systemeinstellungen wie die Dienste und die Registrierungsdateien wieder zu integrieren.";
      Stopwatch w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 1000; i++) {
        string testing = DocumentEncoding.ConvertString(testString);
      }
      w.Start();
      Console.WriteLine(w.ElapsedTicks);
    }

  }

}
