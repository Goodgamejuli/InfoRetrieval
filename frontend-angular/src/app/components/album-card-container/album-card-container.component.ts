import { Component, inject } from '@angular/core';
import { OpenSearchService } from '../../services/opensearch.service';
import { AlbumCardComponent } from '../album-card/album-card.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-album-card-container',
  imports: [AlbumCardComponent, CommonModule],
  templateUrl: './album-card-container.component.html',
  styleUrl: './album-card-container.component.css'
})
export class AlbumCardContainerComponent {
  opensearchService = inject(OpenSearchService);

  // Steuerungsvariablen
  visibleCount: number = 5; // Anzahl der angezeigten Karten
  step: number = 5; // Anzahl, die pro Klick hinzugefügt/entfernt wird

  // Getter, um die sichtbaren alben basierend auf visibleCount zu ermitteln
  get visibleAlbums() {
    return this.opensearchService.albums.slice(0, this.visibleCount);
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
