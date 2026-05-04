# Visual Guide: New Before/After Image Interface

## Layout Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                      DCR Detail Form                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  [Other DCR Fields...]                                          │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Before                          │ After                │   │
│  ├─────────────────────────────────┼─────────────────────┤   │
│  │                                 │                     │   │
│  │  ┌──────┐ ┌──────┐ ┌──────┐    │ ┌──────┐ ┌──────┐   │   │
│  │  │      │ │      │ │      │    │ │      │ │      │   │   │
│  │  │ Img1 │ │ Img2 │ │ Img3 │    │ │ Img1 │ │ Img2 │   │   │
│  │  │      │ │      │ │      │    │ │      │ │      │   │   │
│  │  └──────┘ └──────┘ └──────┘    │ └──────┘ └──────┘   │   │
│  │                                 │                     │   │
│  │  ┌──────┐                       │ ┌──────┐           │   │
│  │  │      │                       │ │      │           │   │
│  │  │ Img4 │                       │ │ Img3 │           │   │
│  │  │      │                       │ │      │           │   │
│  │  └──────┘                       │ └──────┘           │   │
│  │                                 │                     │   │
│  │  [📁 Add Image]                 │ [📁 Add Image]     │   │
│  │                                 │                     │   │
│  └─────────────────────────────────┴─────────────────────┘   │
│                                                                 │
│  [Other Fields and Tabs Below...]                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Empty Gallery State

When no images have been added yet:

```
┌──────────────────────────────────────────┐
│ Before                                   │
├──────────────────────────────────────────┤
│                                          │
│     Drag images here or                  │
│     use buttons below                    │
│                                          │
│                                          │
│     [📁  Add Image]                      │
│     [🗑  Clear]                          │
│                                          │
│                                          │
└──────────────────────────────────────────┘
```

## Thumbnail Interaction States

### Default State (Not Hovered)
```
┌──────────────┐
│              │
│              │  140x140px
│   Image      │  Thumbnail
│   Content    │
│              │
└──────────────┘
```

### Hover State (Remove Button Visible)
```
┌──────────────┐
│ ✕            │  Remove Button
│              │  (hover visible)
│   Image      │
│   Content    │
│              │
└──────────────┘
```

## Multi-Image Gallery Layout

When scrollbar is needed:

```
Before Gallery (with scroll)
┌──────────────────────────────┐
│ ┌──┐ ┌──┐ ┌──┐               │
│ │  │ │  │ │  │               │
│ └──┘ └──┘ └──┘               │ ↑
│ ┌──┐ ┌──┐ ┌──┐               │ │
│ │  │ │  │ │  │               │ │
│ └──┘ └──┘ └──┘               │ │
│ ┌──┐ ┌──┐                    │ │
│ │  │ │  │                    │ ↓
│ └──┘ └──┘                    │
│                              │
│ [📁  Add Image]              │
└──────────────────────────────┘
```

## User Actions

### 1. Adding Images via Dialog

```
User clicks [📁  Add Image]
        ↓
Multi-Select File Dialog Opens
        ↓
User selects 3 image files
        ↓
Dialog closes, images added to gallery
        ↓
Gallery displays 3 thumbnails in flow layout
```

### 2. Drag-and-Drop

```
User drags 2 image files from Explorer
        ↓
Drops into Before gallery area
        ↓
Auto-validates file types
        ↓
Adds thumbnails to gallery
```

### 3. Paste from Clipboard

```
User has image in clipboard (Ctrl+C from screenshot)
        ↓
Clicks in Before gallery (to focus)
        ↓
Presses Ctrl+V
        ↓
Image appears as new thumbnail
```

### 4. Remove Image

```
User hovers over thumbnail
        ↓
Remove button (✕) becomes visible
        ↓
User clicks remove button
        ↓
Thumbnail removed from gallery
```

## Responsive Behavior

### When Gallery Content Exceeds Height

```
┌─────────────────────────────┐
│ ┌──┐ ┌──┐ ┌──┐              │
│ │  │ │  │ │  │              │ ↑
│ └──┘ └──┘ └──┘              │ │
│ ┌──┐ ┌──┐ ┌──┐              │ (scroll)
│ │  │ │  │ │  │              │ │
│ └──┘ └──┘ └──┘              │ ↓
│ ┌──┐ ┌──┐ ┌──┐              │
│ │  │ │  │ │  │              │
│ └──┘ └──┘ └──┘              │
│                             │
│ [📁  Add Image]             │
└─────────────────────────────┘
```

## Interaction with Edit Mode

### View Mode (Read-Only)
```
Gallery is DISABLED (grayed out)
- Add button is disabled
- Drop zone is disabled
- Cannot interact with thumbnails (except view)
- Images are displayed for reference only
```

### Edit/Create Mode (Editable)
```
Gallery is ENABLED (fully interactive)
- All buttons are clickable
- Drag-drop is active
- Paste (Ctrl+V) works
- Remove buttons function
- Changes mark form as dirty
```

## Form State Tracking

```
When image is added/removed:
        ↓
Gallery events fire (ImageAdded / ImageRemoved)
        ↓
Event handlers call MarkDirty()
        ↓
_hasUnsavedChanges = true
        ↓
[Save] button becomes active in toolbar
        ↓
Save prompt appears if user tries to close unsaved
```

## Before/After Sections Side-by-Side

```
┌─────────────────────────────────────────────────────────────┐
│ Before (50% width)      │ After (50% width)                 │
├─────────────────────────┼───────────────────────────────────┤
│ ┌───┐ ┌───┐ ┌───┐      │ ┌───┐ ┌───┐                       │
│ │   │ │   │ │   │      │ │   │ │   │                       │
│ └───┘ └───┘ └───┘      │ └───┘ └───┘                       │
│ ┌───┐                   │ ┌───┐                            │
│ │   │                   │ │   │                            │
│ └───┘                   │ └───┘                            │
│ [Add]                   │ [Add]                            │
└─────────────────────────┴───────────────────────────────────┘
```

## Appearance Details

### Colors
- Gallery background: #fafafa (light gray)
- Border: 1px solid #d0d0d0
- Thumbnail hover: Slight shadow
- Remove button: Red (#dc3545)
- Remove button text: White (#ffffff)

### Typography
- Button text: "📁  Add Image" or custom icon + text
- Hint text: "Drag images here or use buttons below"
- All text: Segoe UI, 9-10pt

### Spacing
- Gallery padding: 8px
- Thumbnail margin: 4px
- Button margin: 4px

## Error States

### Invalid File Type During Drop
```
File dropped but not image format
        ↓
File is skipped silently
        ↓
Other valid files are still added
```

### Large File Load Error
```
User tries to add large file
        ↓
If load fails: Error dialog appears
        ↓
Message: "Failed to load [filename]: [error details]"
        ↓
Gallery remains unchanged
```

## Accessibility

- All controls have clear labels
- Buttons have icons + text
- Focus follows logical order
- Keyboard shortcuts (Ctrl+V for paste)
- Tab navigation through controls
- Screen reader friendly

## Performance

- Thumbnails: 140x140 px max
- Gallery scroll: Smooth and responsive
- No lag with 50+ images
- Memory efficient disposal
