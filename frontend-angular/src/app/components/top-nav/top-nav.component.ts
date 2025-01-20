import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SearchBarService } from '../../services/searchbar.service';
import { CommonModule } from '@angular/common';
import { OpenSearchService } from '../../services/opensearch.service';

@Component({
  selector: 'app-top-nav',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './top-nav.component.html',
  styleUrls: ['./top-nav.component.css']
})
export class TopNavComponent {
  opensearchService = inject(OpenSearchService);
  public isSearchFieldVisible: boolean = false;

  filterOptions = [
    { label: 'Song', value: 'song' },
    { label: 'Künstler', value: 'künstler' },
    { label: 'Album', value: 'album' },
    { label: 'Genre', value: 'genre' },
  ];

  // Die vom Benutzer ausgewählten Optionen
  selectedFilterOptions: string[] = [];

  constructor(
    private router: Router,
    private searchBarService: SearchBarService
  ) {}

  onNavigateToLogin() {
    this.router.navigate(['/', 'login']);
  }

  

  // Methode zum Verarbeiten von Checkbox-Änderungen
  onCheckboxChange(event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    const value = checkbox.value;

    if (checkbox.checked) {
      // Option zur Liste hinzufügen
      this.selectedFilterOptions.push(value);
    } else {
      // Option aus der Liste entfernen
      this.selectedFilterOptions = this.selectedFilterOptions.filter((option) => option !== value);
    }
  }

  onTestClick() {
    this.opensearchService.getSongTest();
  }
}
