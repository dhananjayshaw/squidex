/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AssetDto,
    AssetsState,
    ImmutableArray
} from '@app/shared/internal';

@Component({
    selector: 'sqx-assets-list',
    styleUrls: ['./assets-list.component.scss'],
    templateUrl: './assets-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsListComponent {
    public newFiles = ImmutableArray.empty<File>();

    @Input()
    public state: AssetsState;

    @Input()
    public isDisabled = false;

    @Input()
    public isListView = false;

    @Input()
    public selectedIds: object;

    @Output()
    public selected = new EventEmitter<AssetDto>();

    public add(file: File, asset: AssetDto) {
        this.newFiles = this.newFiles.remove(file);

        this.state.add(asset);
    }

    public search() {
        this.state.load().pipe(onErrorResumeNext()).subscribe();
    }

    public delete(asset: AssetDto) {
        this.state.delete(asset).pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.state.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.state.goPrev().pipe(onErrorResumeNext()).subscribe();
    }

    public update(asset: AssetDto) {
        this.state.update(asset);
    }

    public select(asset: AssetDto) {
        this.selected.emit(asset);
    }

    public isSelected(asset: AssetDto) {
        return this.selectedIds && this.selectedIds[asset.id];
    }

    public remove(file: File) {
        this.newFiles = this.newFiles.remove(file);
    }

    public pasteFiles(event: ClipboardEvent) {
        for (let i = 0; i < event.clipboardData.items.length; i++) {
            const file = event.clipboardData.items[i].getAsFile();

            if (file) {
                this.newFiles = this.newFiles.pushFront(file);
            }
        }
    }

    public addFiles(files: FileList) {
        for (let i = 0; i < files.length; i++) {
            const file = files[i];

            if (file) {
                this.newFiles = this.newFiles.pushFront(file);
            }
        }

        return true;
    }

    public trackByAsset(index: number, asset: AssetDto) {
        return asset.id;
    }
}

