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
    { label: 'Song',      value: 'title',   inputValue: '', hasInput: false},
    { label: 'Album',     value: 'album',   inputValue: '', hasInput: true},
    { label: 'Künstler',  value: 'artist',  inputValue: '', hasInput: true},
    { label: 'Lyrics',    value: 'lyrics',  inputValue: '', hasInput: false},
    { label: 'Genre',     value: 'genre',   inputValue: '', hasInput: true },
    { label: 'Datum',     value: 'date',    inputValue: '', hasInput: true },
  ];

  isExpanded: boolean[] = new Array(this.filterOptions.length).fill(false);

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

   // Umschalten des Inputfelds
   toggleInput(index: number) {
    if (this.filterOptions[index].hasInput) {
      this.isExpanded[index] = !this.isExpanded[index];
    }
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

    // Clear old values
      this.opensearchService.songs = [];
      this.opensearchService.albums = [];
      this.opensearchService.artists = [];

    // Search for songs if selected
    // Search for songs of album if selected
    if(this.filterOptions[1].inputValue != '' && this.selectedFilterOptions.includes('title')) {
      this.opensearchService.searchForSongsInAlbum(this.filterOptions[1].inputValue, this.searchValue, 1);
    }
    // Search for songs of artist
    else if(this.filterOptions[2].inputValue != '' && this.selectedFilterOptions.includes('title')) {
      this.opensearchService.searchForSongsOfArtist(this.filterOptions[2].inputValue, this.searchValue, 1);
    }
    // Search for songs
    else if(this.selectedFilterOptions.includes('title') || this.selectedFilterOptions.includes('lyrics')) {
      if(this.filterOptions[1].inputValue != null && this.filterOptions[4].inputValue != '') 
        this.opensearchService.searchForSongs(this.searchValue, query, this.filterOptions[4].inputValue);
      else
      this.opensearchService.searchForSongs(this.searchValue, query);
    }
      

    // Search for Artists if selected
    if(this.selectedFilterOptions.includes('artist'))
      this.opensearchService.searchForArtist(this.searchValue);

    // Search for albums if selected
    if(this.selectedFilterOptions.includes('album'))
      this.opensearchService.searchForAlbums(this.searchValue);
  }
}
