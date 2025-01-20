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
}
