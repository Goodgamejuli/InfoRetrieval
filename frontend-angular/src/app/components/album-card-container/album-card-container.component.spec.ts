import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AlbumCardContainerComponent } from './album-card-container.component';

describe('AlbumCardContainerComponent', () => {
  let component: AlbumCardContainerComponent;
  let fixture: ComponentFixture<AlbumCardContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlbumCardContainerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AlbumCardContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
