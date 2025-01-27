import { Component, inject } from '@angular/core';
import { OpenSearchService } from '../../services/opensearch.service';
import { ArtistCardComponent } from "../artist-card/artist-card.component";
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-artist-card-container',
  imports: [ArtistCardComponent, CommonModule],
  templateUrl: './artist-card-container.component.html',
  styleUrl: './artist-card-container.component.css'
})
export class ArtistCardContainerComponent {
  opensearchService = inject(OpenSearchService);

  // Steuerungsvariablen
  visibleCount: number = 5; // Anzahl der angezeigten Karten
  step: number = 5; // Anzahl, die pro Klick hinzugefügt/entfernt wird

  // Getter, um die sichtbaren Songs basierend auf visibleCount zu ermitteln
  get visibleArtists() {
    return this.opensearchService.artists.slice(0, this.visibleCount);
  }

  // Show more
  showMore() {
    if (this.visibleCount < this.opensearchService.artists.length) {
      this.visibleCount += this.step;
    }
  }

  // Show less
  showLess() {
    if (this.visibleCount > this.step) {
      this.visibleCount -= this.step;
    }
  }
}
