import { Component, inject, Input } from '@angular/core';
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

  @Input() public isTopResults!: boolean;

  // Steuerungsvariablen
  visibleCount: number = 5; // Anzahl der angezeigten Karten
  step: number = 5; // Anzahl, die pro Klick hinzugefügt/entfernt wird

  // Getter, um die sichtbaren Songs basierend auf visibleCount zu ermitteln
  get visibleSongs() {
    if(this.isTopResults)
      return this.opensearchService.topSongs.slice(0, this.visibleCount)

    return this.opensearchService.songs.slice(0, this.visibleCount);
  }

  // Mehr anzeigen
  showMore() {
    if(this.isTopResults) {
      if (this.visibleCount < this.opensearchService.topSongs.length) {
        this.visibleCount += this.step;
      }
    } 
    else {
      if (this.visibleCount < this.opensearchService.songs.length) {
        this.visibleCount += this.step;
      }
    }
  }

  // Weniger anzeigen
  showLess() {
    if (this.visibleCount > this.step) {
      this.visibleCount -= this.step;
    }
  }
}
