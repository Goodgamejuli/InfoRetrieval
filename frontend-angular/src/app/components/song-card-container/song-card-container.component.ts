import { Component, inject } from '@angular/core';
import { SongCardComponent } from '../song-card/song-card.component';
import { OpenSearchService } from '../../services/opensearch.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-song-card-container',
  imports: [SongCardComponent, CommonModule],
  templateUrl: './song-card-container.component.html',
  styleUrl: './song-card-container.component.css'
})
export class SongCardContainerComponent {
  opensearchService = inject(OpenSearchService);

  // Steuerungsvariablen
  visibleCount: number = 5; // Anzahl der angezeigten Karten
  step: number = 5; // Anzahl, die pro Klick hinzugefügt/entfernt wird

  // Getter, um die sichtbaren Songs basierend auf visibleCount zu ermitteln
  get visibleSongs() {
    return this.opensearchService.songs.slice(0, this.visibleCount);
  }

  // Mehr anzeigen
  showMore() {
    if (this.visibleCount < this.opensearchService.songs.length) {
      this.visibleCount += this.step;
    }
  }

  // Weniger anzeigen
  showLess() {
    if (this.visibleCount > this.step) {
      this.visibleCount -= this.step;
    }
  }
}
