import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { SearchBarService } from '../../services/searchbar.service';
import { CommonModule } from '@angular/common';
import { OpenSearchService } from '../../services/opensearch.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-top-nav',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './top-nav.component.html',
  styleUrls: ['./top-nav.component.css']
})
export class TopNavComponent {
  opensearchService = inject(OpenSearchService);
  public isSearchFieldVisible: boolean = false;

  filterOptions = [
    { label: 'Song', value: 'title' },
    { label: 'Album', value: 'album' },
    { label: 'Künstler', value: 'artist' },
    { label: 'Lyrics', value: 'lyrics' },
    { label: 'Genre', value: 'genre' },
  ];

  searchValue: string = '';

  // Die vom Benutzer ausgewählten Optionen
  selectedFilterOptions: string[] = [];

  constructor(
    private router: Router,
    private searchBarService: SearchBarService
  ) {}

  onNavigateToLogin() {
    this.router.navigate(['/', 'login']);
  }

  

  // Function to check if checkboxes-values are changing
  onCheckboxChange(event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    const value = checkbox.value;

    if (checkbox.checked) {
      this.selectedFilterOptions.push(value);
    } else {
      this.selectedFilterOptions = this.selectedFilterOptions.filter((option) => option !== value);
    }
  }

  // OnClick-function if the search button is pressed
  search() {
    var query = "";

    this.selectedFilterOptions.forEach(selectedFilter => {
      query += selectedFilter + ";";
    });

    console.log(query);

    // If there are no given query parameters, search with all parameters
    if(query=="") {
      this.opensearchService.searchForSongs(this.searchValue);
      return;
    }

    this.opensearchService.searchForSongs(this.searchValue, query);
  }
}
