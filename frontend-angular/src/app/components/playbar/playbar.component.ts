import { Component, inject } from '@angular/core';
import { PlaybarService } from '../../services/playbar.service';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-playbar',
  imports: [],
  templateUrl: './playbar.component.html',
  styleUrl: './playbar.component.css'
})
export class PlaybarComponent {
  playbarService = inject(PlaybarService);

  constructor(private sanitizer: DomSanitizer) {  }

  get songToPlay() {
    if (this.playbarService.songToPlayUrl) {
      return this.sanitizer.bypassSecurityTrustResourceUrl(this.playbarService.songToPlayUrl);
    } else {
      return null;
    }
  }
}
