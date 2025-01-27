import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ArtistCardContainerComponent } from './artist-card-container.component';

describe('ArtistCardContainerComponent', () => {
  let component: ArtistCardContainerComponent;
  let fixture: ComponentFixture<ArtistCardContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ArtistCardContainerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ArtistCardContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
