

<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`AddMyObjectToFactory`**

&nbsp;&nbsp;Signature<br>
```vb
Function AddMyObjectToFactory(x As Integer, y As Integer, z As Integer,
                                                  structureId As Integer) As Integer
```

Creates a new object at the given 3D coordinates and associates it with a specified structure. <br>
Returns a unique identifier for the created object.

&nbsp;&nbsp;Parameters<br>
  Name         | Type     | Description                               
---------------|----------|-------------
 `x`           | Integer  | X coordinate for the new object's location
 `y`           | Integer  | Y coordinate                              
 `z`           | Integer  | Z coordinate                              
 `structureId` | Integer  | The structure group to associate with      

&nbsp;&nbsp;Returns<br>
Integer
<br><br>
Unique ID for the newly created object.

&nbsp;&nbsp;Example Usage<br>
```vb
' Create an object at coordinates (100, 200, 300) associated with structure 5
Dim myObjectId As Integer
myObjectId = api.AddMyObjectToFactory(100, 200, 300, 5)
```

You may call this function multiple times to create multiple objects:<br>
```vb
' Create several objects along the X axis for structure 10
For i = 0 To 9
    api.AddMyObjectToFactory(i, 0, 0, 10)
Next
``` 

&nbsp;&nbsp;Notes<br>
You don't need to track them if you are tracking the structureId. <br>
The return is optional.<br>
The returned integer uniquely identifies the object and can be used for future reference.<br>
The default object color and additional properties are not settable through this API call; only position and structure.<br>
(see MyObject Color Property)


<br><br><br><br>


===<br>
&nbsp;&nbsp;MyObject Color Property<br>
**`obj.ColorOverride`**

&nbsp;&nbsp;Purpose<br>
Objects created via `AddMyObjectToFactory` can have their color overridden by setting the `ColorOverride` property on the `MyObject` instance.<br>
```vb
obj.ColorOverride As ObjectColor?    ' Nullable - set to Nothing for default color
```

---

&nbsp;&nbsp;Setting Color on a Single Object<br>
```vb
' Create object, get ID
Dim objId As Integer = api.AddMyObjectToFactory(x, y, z, structureId)

' Retrieve object from dictionary
Dim obj As MyObject = Nothing
If api.objectDictionary. TryGetValue(objId, obj) Then
    obj.ColorOverride = ObjectColor. Red
End If
```

---

&nbsp;&nbsp;Batch Coloring via Structure<br>
```vb
Dim structureObjIds = DirectCast(api.structureObjectIDs, ConcurrentDictionary(Of Integer, ImmutableList(Of Integer)))
Dim objIdsList As ImmutableList(Of Integer) = Nothing

If structureObjIds.TryGetValue(myStructureId, objIdsList) Then
    For i As Integer = 0 To objIdsList.Count - 1
        Dim obj As MyObject = Nothing
        If api.objectDictionary.TryGetValue(objIdsList(i), obj) Then
            obj.ColorOverride = ObjectColor.Yellow
        End If
    Next
End If
```

---

&nbsp;&nbsp;Clearing Color Override<br>
```vb
obj.ColorOverride = Nothing   ' Returns to default rendering color
```

---

&nbsp;&nbsp;API ObjectColor Enum

```vb
Public Enum ObjectColor
    Black = 1
    White = 2
    WhiteDim = 3
    GreyLight = 4
    Grey = 5
    Brown = 6
    Red = 7
    Orange = 8
    Yellow = 9
    Lime = 10
    Green = 11
    Cyan = 12
    BlueLight = 13
    Blue = 14
    Purple = 15
    Magenta = 16
    Pink = 17
End Enum
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`Bresenham3D`**

&nbsp;&nbsp;Signature<br>
```vb
Function Bresenham3D(startX As Integer, startY As Integer, startZ As Integer,
          endX As Integer, endY As Integer, endZ As Integer) As List(Of (Integer, Integer, Integer))
```

&nbsp;&nbsp;Purpose<br>
Returns all integer grid points along a 3D line from start to end. <br>
Useful for drawing lines, edges, or wireframes in discrete 3D space.

&nbsp;&nbsp;Parameters<br>
  Name    |  Typ      | Description 
----------|-----------|-------------
 `startX` | `Integer` | Starting X coordinate 
 `startY` | `Integer` | Starting Y coordinate 
 `startZ` | `Integer` | Starting Z coordinate 
 `endX`   | `Integer` | Ending X coordinate 
 `endY`   | `Integer` | Ending Y coordinate 
 `endZ`   | `Integer` | Ending Z coordinate 

&nbsp;&nbsp;Returns<br>
```vb
List(Of (Integer, Integer, Integer))
' Each tuple is (X, Y, Z) for a point along the line
```

&nbsp;&nbsp;Plugin use cases<br>
Generate points for drawing 3D edges/wireframes. <br>
Place objects along a line segment between two vertices. 

&nbsp;&nbsp;Plugin access pattern<br>
Draw a line:<br>
```vb
Dim pts = api. Bresenham3D(startX, startY, startZ, endX, endY, endZ)

For Each pt In pts
    api.AddMyObjectToFactory(pt.Item1, pt. Item2, pt.Item3, myStructureId)
Next
```

Coordinates must be integers; round floating-point values before calling.

Returns points inclusive of both start and end. 

Works in any direction (handles negative slopes automatically).


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`ThinEvenSpatiallyAdaptiveAuto`**

&nbsp;&nbsp;Signature<br>
```vb
Sub ThinEvenSpatiallyAdaptiveAuto(
    ByRef sourceDict As ConcurrentDictionary(Of Integer, MyObject),
    ByRef destDict As ConcurrentDictionary(Of Integer, MyObject),
    numToLeave As Integer,
    observer As (Integer, Integer, Integer),
    keepRadius As Double,
    Optional numBands As Integer = 10,
    Optional closeBiasExponent As Double = 1.5)
```

&nbsp;&nbsp;Purpose<br>
Thins a fully constructed scene down to `numToLeave` objects, preserving spatial distribution.<br>
Objects within `keepRadius` of the observer are always retained.<br>
Objects beyond `keepRadius` are grouped into distance bands; closer bands retain more objects.<br>
**Call once after scene construction, before frame processing begins.**

&nbsp;&nbsp;Parameters<br>
  Name               |  Type                                        | Description 
---------------------|----------------------------------------------|-------------
 `sourceDict`        | `ConcurrentDictionary(Of Integer, MyObject)` | Collection to thin; modified in-place 
 `destDict`          | `ConcurrentDictionary(Of Integer, MyObject)` | Unused; pass `Nothing` 
 `numToLeave`        | `Integer`                                    | Target object count to retain 
 `observer`          | `(Integer, Integer, Integer)`                | Reference point for distance calculations 
 `keepRadius`        | `Double`                                     | Objects within this distance are always kept 
 `numBands`          | `Integer`                                    | Distance bands beyond `keepRadius` (default: 10) 
 `closeBiasExponent` | `Double`                                     | Higher = more objects kept near observer (default: 1.5) 

&nbsp;&nbsp;Plugin usage<br>
```vb
api.ThinEvenSpatiallyAdaptiveAuto(
    api.objectDictionary, Nothing, 30000,
    api.GetObserverOrigin(), 200, 10, 1.5)
```

&nbsp;&nbsp;When to call<br>
Once, after all scene objects are constructed. 

&nbsp;&nbsp;Algorithm summary<br>
1. Objects within `keepRadius` -> always kept.<br>
2. Objects beyond -> assigned to distance bands. <br>
3. Each band gets a weighted share of `numToLeave` (closer bands weighted higher).<br>
4. Within each band, a 3D grid bins objects; one per cell is kept.<br>
5. Remaining quota filled randomly; unselected objects removed from `sourceDict`.

If `sourceDict.Count <= numToLeave`, no thinning occurs.

`destDict` is currently unused; pass `Nothing`.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`AddTriangle`**

&nbsp;&nbsp;Signature<br>
```vb
Function AddTriangle(
    x1 As Double, y1 As Double, z1 As Double,
    x2 As Double, y2 As Double, z2 As Double,
    x3 As Double, y3 As Double, z3 As Double,
    setId As Integer) As Integer
```

&nbsp;&nbsp;Purpose<br>
Registers a triangle for ray-occlusion testing.<br>
Groups the triangle under `setId` for batch management.

&nbsp;&nbsp;Parameters<br>
  Name        | Type     | Description 
--------------|----------|-------------
 `x1, y1, z1` | `Double` | Vertex A position (X, Y, Z) 
 `x2, y2, z2` | `Double` | Vertex B position (X, Y, Z) 
 `x3, y3, z3` | `Double` | Vertex C position (X, Y, Z) 
 `setId`      | `Integer`| Group identifier for this triangle 

&nbsp;&nbsp;Returns<br>
`Integer`: Unique `triangleId` assigned to the new triangle. 

&nbsp;&nbsp;Plugin use cases<br>
Define occlusion surfaces for your objects.<br>
Update geometry each frame for moving/rotating objects.

&nbsp;&nbsp;Plugin usage<br>
```vb
Private triangleSetId As Integer = 0
Private topLeft As (Double, Double, Double)
Private topRight As (Double, Double, Double)
Private bottomLeft As (Double, Double, Double)
Private bottomRight As (Double, Double, Double)

Public Sub UpdateTriangles()
    ' Clear existing triangles if set already exists
    If triangleSetId > 0 Then
        api.RemoveAllTrianglesInSet(triangleSetId)
    End If

    ' Initialize setId if not yet assigned
    If triangleSetId = 0 Then
        triangleSetId = api.GetNextUniqId() 
    End If

    ' Alias corners for readability
    Dim a = topLeft
    Dim b = topRight
    Dim c = bottomRight
    Dim d = bottomLeft

    ' First triangle (top-left, top-right, bottom-right)
    api.AddTriangle(
        a.Item1, a.Item2, a.Item3,
        b.Item1, b.Item2, b.Item3,
        c.Item1, c.Item2, c.Item3,
        triangleSetId)

    ' Second triangle (top-left, bottom-right, bottom-left)
    api.AddTriangle(
        a.Item1, a.Item2, a.Item3,
        c.Item1, c.Item2, c.Item3,
        d.Item1, d.Item2, d.Item3,
        triangleSetId)
End Sub
```

&nbsp;&nbsp;Notes<br>
Use a stable `setId` per object/structure. <br>
Returned `triangleId` can be used with `trianglesById` for lookups.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetMyObjectByStructureId`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetMyObjectByStructureId(structureId As Integer) As MyObject
```

&nbsp;&nbsp;Purpose<br>
Retrieves a `MyObject` instance from the host's object dictionary by its structure ID.<br>
Returns `Nothing` if no object exists for the given ID.

&nbsp;&nbsp;From the API<br>
```vb
Public Class MyObject
    Public Property UniqIdentifier As Integer
    Public Property StructureId As Integer
    Public Property Location As Coordinates3D
    Public Property ColorOverride As ObjectColor?

    Public Sub New(location As Coordinates3D, key As Integer, structureId As Integer)
        Me.Location = location
        Me.UniqIdentifier = key
        Me.StructureId = structureId
        Me.ColorOverride = Nothing
    End Sub

    Public Sub UpdateLocation(newX As Long, newY As Long, newZ As Long)
        Me.Location.X = newX
        Me.Location.Y = newY
        Me.Location.Z = newZ
    End Sub
End Class

Public Class Coordinates3D
    Public Property X As Long
    Public Property Y As Long
    Public Property Z As Long
    Public Sub New(x As Long, y As Long, z As Long)
        Me.X = x
        Me.Y = y
        Me.Z = z
    End Sub
End Class
```

&nbsp;&nbsp;Example plugin use cases<br>
Retrieve an object to inspect or modify its properties. <br>
Check if an object exists before performing operations on it.

&nbsp;&nbsp;Plugin access patterns<br>
Retrieve an object by structure ID:<br>
```vb
Dim obj As MyObject = api.GetMyObjectByStructureId(myStructureId)
If obj IsNot Nothing Then
    ' Object exists, use it
End If
```

Check existence before operating:<br>
```vb
Dim obj As MyObject = api.GetMyObjectByStructureId(myStructureId)
If obj IsNot Nothing Then
    obj.UpdateLocation(newX, newY, newZ)
End If
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetNextUniqId`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetNextUniqId() As Integer
```

&nbsp;&nbsp;Purpose<br>
Returns a globally unique integer ID. <br>
Thread-safe; uses `Interlocked.Increment` internally.

&nbsp;&nbsp;Return<br>
`Integer`: next available unique ID (increments with each call).

&nbsp;&nbsp;Plugin use cases<br>
Generate unique keys for custom dictionaries or tracking structures.<br>
Create stable IDs for plugin-managed objects that need cross-reference.

&nbsp;&nbsp;Plugin access pattern<br>
Get a unique ID:<br>
```vb
Dim myUniqueId As Integer = api.GetNextUniqId()
```

IDs are sequential and never recycled within a session.<br>
Safe to call from multiple threads concurrently.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Methods<br>
**`GetObserverOrigin`**<br>
**`GetObserverUnitVector`**

Both functions can be used independently or in combination for powerful spatial operations.

&nbsp;&nbsp;Signature<br>
```vb
Function GetObserverOrigin() As (Integer, Integer, Integer)
```

&nbsp;&nbsp;Returns<br>
The observer's 3D position as a tuple `(X, Y, Z)`.

&nbsp;&nbsp;Solo Usage Examples<br>
Query Current Location:<br>
```vb
   Dim pos = api.GetObserverOrigin()
   Console.WriteLine($"Observer is at: ({pos.Item1}, {pos.Item2}, {pos.Item3})")
```

Region Check:<br>
```vb
   Dim pos = api.GetObserverOrigin()
   If pos.Item1 >= 0 And pos.Item2 >= 0 And pos.Item3 >= 0 Then
       Console.WriteLine("Observer is in the positive region.")
   End If
```

&nbsp;&nbsp;Signature<br>
```vb
Function GetObserverUnitVector() As (Double, Double, Double)
```

&nbsp;&nbsp;Returns<br>
The observer's facing direction as a unit vector `(X, Y, Z)`.

Solo Usage Example<br>
```vb
Query Facing Direction
   Dim dir = api.GetObserverUnitVector()
   Console.WriteLine($"Observer faces: ({dir.Item1}, {dir.Item2}, {dir.Item3})")
```

Use Together<br>
It's common to use both origin and unit vector for spatial operations requiring both an origin and a direction.

Examples<br>
Raycasting / Picking:<br>
```vb
   Dim origin = api.GetObserverOrigin()
   Dim direction = api.GetObserverUnitVector()
   Dim hit = Raycast(origin, direction)
```

Move Observer Forward:<br>
```vb
   Dim origin = api.GetObserverOrigin()
   Dim direction = api.GetObserverUnitVector()
   ' Move observer 10 units ahead
   Dim newPos = (
     origin.Item1 + direction.Item1 * 10,
     origin.Item2 + direction.Item2 * 10,
     origin.Item3 + direction.Item3 * 10
   )
```

Place Marker in View:<br>
```vb
   Dim origin = api.GetObserverOrigin()
   Dim direction = api.GetObserverUnitVector()
   ' Marker 5 units directly in front of observer
   Dim markerPos = (
     origin.Item1 + direction.Item1 * 5,
     origin.Item2 + direction.Item2 * 5,
     origin.Item3 + direction.Item3 * 5
   )
   PlaceMarker(markerPos)
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Methods (Panel Bounds)<br>
**`GetPanelFurthestLeftColumn`**<br>
**`GetPanelFurthestTopRow`**<br>
**`GetPanelFurthestRightColumn`**<br>
**`GetPanelFurthestBottomRow`**

These API functions let your plugin query the visible grid boundaries of any panel in the 3D workspace:

&nbsp;&nbsp;Signatures<br>
```vb
' Returns the index of the furthest (minimum) column of the panel grid.
Function GetPanelFurthestLeftColumn(panel As PanelType) As Integer

' Returns the index of the furthest (minimum) row of the panel grid.
Function GetPanelFurthestTopRow(panel As PanelType) As Integer

' Returns the index of the furthest (maximum) column of the panel grid.
Function GetPanelFurthestRightColumn(panel As PanelType) As Integer

' Returns the index of the furthest (maximum) row of the panel grid.
Function GetPanelFurthestBottomRow(panel As PanelType) As Integer
```

&nbsp;&nbsp;Parameters<br>
`panel As PanelType`<br>
 Valid values: `TopPanel`, `BottomPanel`, `NorthPanel`, `SouthPanel`, `EastPanel`, `WestPanel`

&nbsp;&nbsp;Tip<br>
Use these methods to locate the edges ("walls") of a panel when laying out margins, grid objects, or managing adjacency.

&nbsp;&nbsp;Example Usage<br>
```vb
Dim leftCol   As Integer = api.GetPanelFurthestLeftColumn(PanelType.TopPanel)
Dim topRow    As Integer = api.GetPanelFurthestTopRow(PanelType.TopPanel)
Dim rightCol  As Integer = api.GetPanelFurthestRightColumn(PanelType.TopPanel)
Dim bottomRow As Integer = api.GetPanelFurthestBottomRow(PanelType.TopPanel)
```

This example retrieves the full grid limits for the Top panel.

&nbsp;&nbsp;API Enum<br>
```vb
Public Enum PanelType
    BottomPanel
    NorthPanel
    EastPanel
    SouthPanel
    WestPanel
    TopPanel
End Enum
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Methods<br>
**`AssignZoneMarginSetA`**<br>
**`AssignZoneMarginSetB`**

Assign named *margin sets* to either the Primary (A) or Secondary (B) "slot" for a spatial zone.<br>
Later, you can swap or activate these slots independently for flexible layout/state control.

**IMPORTANT:** The "A" and "B" in these method names refer to *slots*, not to how you should name your margin sets! You can assign margin sets with *any name* to either slot. 

&nbsp;&nbsp;Signatures<br>
```vb
Sub AssignZoneMarginSetA(zoneId As String, setName As String)
Sub AssignZoneMarginSetB(zoneId As String, setName As String)
```
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`zoneId` = ID of the target spatial zone.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`setName` = Name of *any* margin set you've created (can be any valid name).

&nbsp;&nbsp;Usage<br>
Create Margin Sets<br>
Use host API to define and name your margin sets.<br>
Assign Margin Sets to Zone Slots<br>
```vb


api.AssignZoneMarginSetA("zone1", "MainLayout") ' Assign "MainLayout" to the Primary Slot (A)
api.AssignZoneMarginSetB("zone1", "AltLayout") '  Assign "AltLayout" to the Secondary Slot (B)
```
"MainLayout" is now in Primary Slot (A); "AltLayout" in Secondary Slot (B) for zone "zone1".

**Activate or Swap Slots (Not covered here)**<br>
&nbsp;&nbsp;&nbsp;&nbsp;Use host API to activate or swap which slot is currently displayed:<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`SwitchZoneToMarginSetA(zoneId)`   Activate the Primary Slot<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`SwitchZoneToMarginSetB(zoneId)`   Activate the Secondary Slot<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`SwapZoneMarginSets(zoneId)`  ' toggle between Primary and Secondary

&nbsp;&nbsp;Concept<br>
Each spatial zone has **two slots** (Primary and Secondary), each holding a margin set name.<br>
You can *assign*, *activate*, or *swap* which slot is currently in use.<br>
"Swapping" only toggles which slot is active-it does NOT change the slot contents.

&nbsp;&nbsp;Margin sets<br>
Must be created first.<br>
**Names can be anything meaningful to your application.**<br>
Slots are just pointers to margin set names-swapping doesn't modify the sets.

&nbsp;&nbsp;Quick Sample<br>
```vb
' Create your margin sets with descriptive names
api.CreateMarginSet("MyFirstLayout", ...)
api.CreateMarginSet("MySecondLayout", ...)

' Assign them to the zone's two slots
api.AssignZoneMarginSetA("zoneX", "MyFirstLayout")   ' Primary Slot
api.AssignZoneMarginSetB("zoneX", "MySecondLayout")  ' Secondary Slot

' Switch between them as needed
api.SwitchZoneToMarginSetA("zoneX")  ' Show MyFirstLayout
api.SwitchZoneToMarginSetB("zoneX")  ' Show MySecondLayout
api.SwapZoneMarginSets("zoneX")      ' Toggle to the other slot
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`CreateMarginSet`**

&nbsp;&nbsp;Signature<br>
```vb
Sub CreateMarginSet(setName As String, topRowMarginId As String, bottomRowMarginId As String,
                                       leftColumnMarginId As String, rightColumnMarginId As String)
```

&nbsp;&nbsp;Purpose<br>
Creates or overwrites a **named margin set** associating margin identifiers for layout purposes.<br>
A margin set consists of **two row margins (top/bottom)** and **two column margins (left/right)**.

&nbsp;&nbsp;Usage<br>
```vb
' Create top/bottom/left/right margins first (not shown here)
api.CreateMarginSet("MyLayout", "MyLayout_TopRow", "MyLayout_BottomRow",
                                "MyLayout_LeftColumn", "MyLayout_RightColumn")
```
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`setName`: Arbitrary name for this logical grouping (e.g., "MyLayout", "CustomMargins", "LayoutOne").<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`topRowMarginId`: Name/id of top row margin (as created before).<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`bottomRowMarginId`: Name/id of bottom row margin.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`leftColumnMarginId`: Name/id of left column margin.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`rightColumnMarginId`: Name/id of right column margin.

Calling `CreateMarginSet` **overwrites** any set with the same name.

&nbsp;&nbsp;Margin IDs<br>
Margins must be created first, e.g., `'MyLayout_TopRow'` or `'Zone3_LeftColumn'`, using `api.CreateMargin`.<br>
Margin IDs are unique, descriptive strings made during margin creation. 

&nbsp;&nbsp;Example Pattern<br>
Step 1: Create margins (names must match your Set call)<br>
```vb
api.CreateMargin("AnotherLayout_TopRow", MarginType. RowMargin, PanelType.TopPanel, 40, Nothing, False)
api.CreateMargin("AnotherLayout_BottomRow", MarginType. RowMargin, PanelType.TopPanel, 50, Nothing, False)
api.CreateMargin("AnotherLayout_LeftColumn", MarginType. ColumnMargin, PanelType.TopPanel, Nothing, 22, False)
api.CreateMargin("AnotherLayout_RightColumn", MarginType. ColumnMargin, PanelType.TopPanel, Nothing, 35, False)
```

Step 2: Create the margin set<br>
```vb
api.CreateMarginSet("AnotherLayout", "AnotherLayout_TopRow", "AnotherLayout_BottomRow",
                                     "AnotherLayout_LeftColumn", "AnotherLayout_RightColumn")
```

**Overwrites**: If `setName` already exists, it is replaced.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`CreateSpatialZone`**

&nbsp;&nbsp;Signature<br>
```vb
Function CreateSpatialZone(zoneId As String) As ISpatialZone
```

&nbsp;&nbsp;Purpose<br>
Creates (or retrieves) a named spatial zone for rendering text and UI elements on a panel. <br>
Returns an `ISpatialZone` interface for configuring and querying the zone. 

&nbsp;&nbsp;Parameters<br>
  Name    |  Type    | Description 
----------|----------|-------------
 `zoneId` | `String` | Unique identifier for the zone 

&nbsp;&nbsp;Returns<br>
`ISpatialZone` - adapter exposing zone properties and methods. 

ISpatialZone Members<br>
Properties (read-only unless noted)<br>
 Name           |  Type                                                        | Description 
--------------------|--------------------------------------------------------------|-------------
 `ID`               | `String`                                                     | The zone's unique identifier 
 `Left`             | `Integer`                                                    | Left boundary (column index) 
 `Right`            | `Integer`                                                    | Right boundary (column index) 
 `Top`              | `Integer`                                                    | Top boundary (row index) 
 `Bottom`           | `Integer`                                                    | Bottom boundary (row index) 
 `Text`             | `String`                                                     | *Read/Write* - displayed text content 
 `WrappedCharIndex` | `Dictionary(Of (Integer, Integer), Char)`                    | Maps `(row, col)` to character 
 `BoundingBoxAABB`  | `((Integer, Integer, Integer), (Integer, Integer, Integer))` | Min/max 3D corners 

Methods<br>
  Method                                               | Description 
-------------------------------------------------------|-------------
 `UpdateMargins(leftId, rightId, topId, bottomId)`     | Reposition zone using margin IDs 
 `GetAllFontSegments() As List(Of (Integer, Integer))` | Returns all `(row, col)` font-cell positions 
 `SetGutterVisible(row, col, side, visible)`           | Show/hide gutter on `"above"`, `"below"`, `"left"`, or `"right"` 
 `DisposeZone()`        <-- ** DO NOT USE **           | Clean up zone resources  ** WIP just set margins all 0/Nothing Nothing/0

&nbsp;&nbsp;Plugin Usage<br>
Create a zone and set text<br>
```vb
Dim zone As ISpatialZone = api.CreateSpatialZone("MyZone")
zone.Text = "Hello World"
```

Position the zone using margins ** Depreciated(internal only) Use A/B Sets.<br>
```vb
zone.UpdateMargins("LeftMarginId", "RightMarginId", "TopMarginId", "BottomMarginId")
```

Query zone bounds<br>
```vb
Dim left As Integer = zone.Left
Dim right As Integer = zone.Right
Dim top As Integer = zone.Top
Dim bottom As Integer = zone.Bottom
```

Get the 3D bounding box<br>
```vb
Dim aabb = zone.BoundingBoxAABB
Dim minCorner = aabb.Item1  ' (minX, minY, minZ)
Dim maxCorner = aabb.Item2  ' (maxX, maxY, maxZ)
```

Iterate characters by grid position<br>
```vb
For Each kvp In zone.WrappedCharIndex
    Dim row As Integer = kvp.Key. Item1
    Dim col As Integer = kvp.Key.Item2
    Dim ch As Char = kvp.Value
    ' Process character at (row, col)
Next
```

Get all font segment positions<br>
```vb
Dim segments As List(Of (Integer, Integer)) = zone.GetAllFontSegments()
For Each seg In segments
    Dim segRow As Integer = seg.Item1
    Dim segCol As Integer = seg.Item2
Next
```

Toggle gutter visibility<br>
```vb
zone.SetGutterVisible(0, 1, "above", True)   ' Show gutter above cell (0,1)
zone.SetGutterVisible(0, 1, "above", False)  ' Hide it
```

&nbsp;&nbsp;What Happens if I Remove a Zone?<br>
If you call `api.RemoveSpatialZone(zoneId)`, the host disposes the zone.<br>
Your plugin reference to the zone object may still exist ("zombie" zone)-use with caution! WIP<br>
Operations after removal may not be tracked by the host and can lead to resource leaks or surprising behavior.

**Use zone references only while zone is alive/managed.**

&nbsp;&nbsp;Notes<br>
Zone is created once per `zoneId`; subsequent calls return the existing zone.<br>
Collision triangles are managed internally via `UpdateCollisionTriangles()` when margins change.<br>
Treat returned `ISpatialZone` as your control surface; internal implementation is hidden.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetSpatialZone`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetSpatialZone(zoneId As String) As ISpatialZone
```

&nbsp;&nbsp;Purpose<br>
Retrieves an existing spatial zone by its ID.

&nbsp;&nbsp;Return<br>
ISpatialZone   ' or Nothing if not found

&nbsp;&nbsp;Plugin use cases<br>
Retrieve a zone created earlier (via `api.CreateSpatialZone`).<br>
Access zone properties (bounds, text, bounding box).<br>
Manipulate zone state (margins, text, gutter visibility).<br>
Build a zone pool for reuse across plugin logic.

&nbsp;&nbsp;Plugin access patterns<br>
Retrieve a zone:<br>
```vb
Dim zone As ISpatialZone = api. GetSpatialZone("myZoneId")
If zone IsNot Nothing Then
    ' Zone exists, use it
End If
```

Access zone properties:<br>
```vb
Dim zone As ISpatialZone = api.GetSpatialZone("myZoneId")
If zone IsNot Nothing Then
    Dim left As Integer = zone.Left
    Dim right As Integer = zone.Right
    Dim top As Integer = zone. Top
    Dim bottom As Integer = zone.Bottom
    Dim id As String = zone.ID
    Dim aabb = zone.BoundingBoxAABB   ' ((minX, minY, minZ), (maxX, maxY, maxZ))
End If
```

Read/write zone text:<br>
```vb
Dim zone As ISpatialZone = api.GetSpatialZone("myZoneId")
If zone IsNot Nothing Then
    zone.Text = "Hello World"
    Dim currentText As String = zone.Text
End If
```

&nbsp;&nbsp;ISpatialZone members available<br>
  Member                 |  Type                            | Description 
-------------------------|----------------------------------|-------------
 `ID`                    | `String`                         | Zone identifier 
 `Left`                  | `Integer`                        | Left column boundary 
 `Right`                 | `Integer`                        | Right column boundary 
 `Top`                   | `Integer`                        | Top row boundary 
 `Bottom`                | `Integer`                        | Bottom row boundary 
 `Text`                  | `String`                         | Get/set zone text content 
 `BoundingBoxAABB`       | `((Int,Int,Int),(Int,Int,Int))`  | 3D bounding box (min, max) 
 `WrappedCharIndex`      | `Dictionary(Of (Int,Int), Char)` | Character positions 
 `GetAllFontSegments()`  | `List(Of (Int,Int))`             | Font grid segments 
 `UpdateMargins(...)`    | `Sub`                            | Update zone margin references 
 `SetGutterVisible(...)` | `Sub`                            | Control gutter visibility 
 `DisposeZone()`         | `Sub`                            | Clean up zone resources WIP 


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetAllMarginSetNames`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetAllMarginSetNames() As List(Of String)
```

&nbsp;&nbsp;Returns<br>
A list of strings with the names of all currently defined margin sets in the host.

&nbsp;&nbsp;Usage<br>
Get all available margin set names:<br>
```vb
Dim marginSets = api.GetAllMarginSetNames()
' Example output: ["SetA", "SetB", ...]
Console.WriteLine("Available margin sets: " & String.Join(", ", marginSets))
```

&nbsp;&nbsp;Context: What is a Margin Set?<br>
A *margin set* groups named "margin" objects, typically for layout or spatial zone customization.<br>
Each set is referenced by a unique string name.<br>
Useful for toggling layouts or configurations (see "A/B margin sets" design elsewhere).

&nbsp;&nbsp;**Tip:**<br>
Margin set names are shared across spatial zones and plugin instances.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetAllMarginIDs`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetAllMarginIDs() As Dictionary(Of String, List(Of String))
```

&nbsp;&nbsp;Purpose<br>
Retrieves all registered margin IDs, categorized by type (row or column). 

&nbsp;&nbsp;Return type<br>
```vb
Dictionary(Of String, List(Of String))
```
&nbsp;&nbsp;&nbsp;&nbsp;Key:    "row" or "column"<br>
&nbsp;&nbsp;&nbsp;&nbsp;Value:  List of margin ID strings

&nbsp;&nbsp;Plugin use cases<br>
Discover all existing margins in the host.<br>
Enumerate row margins separately from column margins.<br>
Use retrieved IDs with other margin-related API calls.

&nbsp;&nbsp;Plugin access patterns<br>
Get all margin IDs:<br>
```vb
Dim allMargins = api.GetAllMarginIDs()
Dim rowMargins As List(Of String) = allMargins("row")
Dim columnMargins As List(Of String) = allMargins("column")
```

Check if any row margins exist:<br>
```vb
Dim allMargins = api.GetAllMarginIDs()
If allMargins("row").Count > 0 Then
    ' Row margins exist
End If
```

Iterate all row margin IDs:<br>
```vb
Dim allMargins = api. GetAllMarginIDs()
For Each marginId As String In allMargins("row")
    ' Use marginId with other margin API calls
Next
```

Iterate all column margin IDs:<br>
```vb
Dim allMargins = api.GetAllMarginIDs()
For Each marginId As String In allMargins("column")
    ' Use marginId with other margin API calls
Next
```

Returns a snapshot; margins added/removed after the call won't appear. 


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetAllSpatialZones`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetAllSpatialZones() As IEnumerable(Of ISpatialZone)
' Returns all current zones managed by the host.
```

Returns a list (or enumerable) of spatial zone objects (`ISpatialZone`).

&nbsp;&nbsp;Typical Usage<br>
```vb
For Each zone As ISpatialZone In api.GetAllSpatialZones()
    Console.WriteLine(zone.ID & ": " & zone.Text)
Next
```

&nbsp;&nbsp;What is an `ISpatialZone`?<br>
Returned objects support:<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**Properties:**<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`Left`, `Right`, `Top`, `Bottom` *(Integer)*: Boundaries of this zone.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`Text` *(String, RW)*: Info/label field for display or annotation.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ID` *(String)*: Unique zone identifier.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;**Methods:**<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`UpdateMargins(leftId, rightId, topId, bottomId)`: Change zone margin assignment.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`DisposeZone()`: Remove the zone from host management (see below tip).<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`GetAllFontSegments()`: List of font grid segments (tuples).<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`SetGutterVisible(row, col, side, visible)`: Show/hide gutter for a cell-side.

(Quick Notes)<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Use `api.GetAllSpatialZones()` to inspect all zones currently managed by the host.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Each returned zone exposes margins, boundaries, and customizable text.<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Disposing a zone stops host tracking-but .NET references may still affect state.WIP


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetMarginInfoSnapshot`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetMarginInfoSnapshot(marginId As String) As Dictionary(Of String, Object)
```

&nbsp;&nbsp;Purpose<br>
Retrieve a snapshot of margin properties for a given margin ID.

&nbsp;&nbsp;Returns<br>
```vb
Dictionary(Of String, Object)` containing margin details.
```
If `marginId` is invalid/unknown, returns `Nothing`.

&nbsp;&nbsp;Structure<br>
The returned dictionary may include the following keys (properties are all present,<br>
but `Row`/`Column` may be `Nothing` depending on margin type):<br>
  Key          | Type                  | Description                                          
 ------------- | --------------------- | ------------
 `ID`          | String                | Margin unique ID (name).                             
 `Type`        | MarginType Enum       | Either `RowMargin` or `ColumnMargin`.                
 `Panel`       | PanelType Enum        | Panel location (`TopPanel`, `WestPanel`, etc.).      
 `Locked`      | Boolean               | True if margin cannot be moved/removed.              
 `StructureID` | Integer               | Structure identifier for visibility toggling.       
 `Row`         | Integer? (nullable)   | Row index if type is `RowMargin`, else `Nothing`.    
 `Column`      | Integer? (nullable)   | Column index if type is `ColumnMargin`, else `Nothing`.

&nbsp;&nbsp;Usage Example<br>
```vb
Dim marginInfo = api.GetMarginInfoSnapshot("RightColumn")

' Sample property access:
Dim isLocked As Boolean = CBool(marginInfo("Locked"))
Dim marginType As MarginType = CType(marginInfo("Type"), MarginType)
Dim panel As PanelType = CType(marginInfo("Panel"), PanelType)
Dim maybeRow As Integer? = CType(marginInfo("Row"), Integer?)
Dim maybeCol As Integer? = CType(marginInfo("Column"), Integer?)
```

&nbsp;&nbsp;API Enums<br>
**PanelType**:<br>
`BottomPanel`, `NorthPanel`, `EastPanel`, `SouthPanel`, `WestPanel`, `TopPanel`

**MarginType**:<br>
`RowMargin`, `ColumnMargin`


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetMarginSet`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetMarginSet(setName As String) As Dictionary(Of String, String)
```

&nbsp;&nbsp;Purpose<br>
Retrieve the contents of a named margin set.

&nbsp;&nbsp;Usage<br>
Input<br>
`setName`: Name of the margin set (as specified during setup). This can be any name you chose when creating the set.

&nbsp;&nbsp;Returns<br>
`Dictionary(Of String, String)` containing margin assignments:<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Always includes keys:  `"TopRow"`, `"BottomRow"`, `"LeftColumn"`, `"RightColumn"`<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Each value is the margin ID used by the set. 

&nbsp;&nbsp;Example<br>
```vb
    Dim marginSet = api.GetMarginSet("MyLayout")
    ' marginSet("TopRow")       => the margin ID for the top row
    ' marginSet("BottomRow")    => the margin ID for the bottom row
    ' marginSet("LeftColumn")   => the margin ID for the left column
    ' marginSet("RightColumn")  => the margin ID for the right column
```

Use in conjunction with other host API features to achieve zone layout swapping, alternate views, or custom UI logic.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`GetZoneAssignedMarginSet`**

&nbsp;&nbsp;Signature<br>
```vb
Function GetZoneAssignedMarginSet(zoneId As String) As String
```

&nbsp;&nbsp;Purpose<br>
Returns the name of the _currently active margin set_ assigned to the specified spatial zone.

&nbsp;&nbsp;Input<br>
`zoneId` (String): The ID of the spatial zone you wish to query.

&nbsp;&nbsp;Returns<br>
(String): Name of the active margin set (e.g., `"MainLayout"`, `"MyCustomSet"`, etc.), or `Nothing` if the zone is not found or no set is assigned.

**Note:** This returns whatever name *you* gave your margin set when you created it.  The return value is not related to the slot names (A/B)

&nbsp;&nbsp;Usage<br>
```vb
Dim currentSet As String = api.GetZoneAssignedMarginSet("zone1")
Console.WriteLine("Active margin set for zone1: " & currentSet)
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`CreateMargin`**

Add margins into the host application.

&nbsp;&nbsp;Signature<br>
```vb
Sub CreateMargin(
    marginId As String,
    marginType As MarginType,
    panel As PanelType,
    row As Integer?,
    column As Integer?,
    locked As Boolean
)
```

&nbsp;&nbsp;Parameters<br>
 Name        | Type          | Description 
-------------|---------------|-------------
 marginId    | String        | Unique identifier for the new margin. Ensure this is unique across the project. 
 marginType  | MarginType    | `RowMargin` or `ColumnMargin`. Selects which axis the margin relates to.
 panel       | PanelType     | Target panel (see host for available options).
 row         | Integer?      | The row index for a `RowMargin`. **Must be omitted (`Nothing`) for `ColumnMargin`.**
 column      | Integer?      | The column index for a `ColumnMargin`. **Must be omitted (`Nothing`) for `RowMargin`.** 
 locked      | Boolean       | If `True`, margin is locked (not editable by users). 


&nbsp;&nbsp;API Enum<br>
```vb
Public Enum MarginType
    RowMargin
    ColumnMargin
End Enum
```

&nbsp;&nbsp;API Enum<br>
```vb
Public Enum PanelType
    BottomPanel
    NorthPanel
    EastPanel
    SouthPanel
    WestPanel
    TopPanel
End Enum
```

&nbsp;&nbsp;Example Usages<br>
Adding a Row Margin:<br>
```vb
CreateMargin(
    marginId:="my_unique_row_margin",
    marginType:=MarginType.RowMargin,
    panel:=PanelType.MainPanel,
    row:=2,
    column:=Nothing,
    locked:=False
)
```

Adding a Column Margin:<br>
```vb
CreateMargin(
    marginId:="my_unique_column_margin",
    marginType:=MarginType.ColumnMargin,
    panel:=PanelType.MainPanel,
    row:=Nothing,
    column:=1,
    locked:=False
)
```

Margins are not visible by default on creation. ** WIP dissabled for latency. waiting for drawing tools.

Each `marginId` must be unique.

Once created with `locked:=True`, the margin cannot be edited by users until unlocked.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`MarginPlusOne`**

Incrementally move margins (either by row or by column) within the panel structure.

&nbsp;&nbsp;Signature<br>
```vb
Sub MarginPlusOne(marginId As String)
```

MarginPlusOne moves a margin (either a row or a column margin) forward by one position in its respective direction:<br>
&nbsp;&nbsp;&nbsp;&nbsp;For **RowMargin**, it moves the margin to the next row.<br>
&nbsp;&nbsp;&nbsp;&nbsp;For **ColumnMargin**, it moves the margin to the next column.

If moving the margin goes out of range for the current panel,<br>
the API automatically wraps the margin to the adjacent panel on the appropriate side<br>
(top/bottom for rows, left/right for columns) using 3D cube adjacency logic.

The API internally checks the `Margin.Type` to choose to increment the row or the column.

&nbsp;&nbsp;Example Usages<br>
Move a Row Margin forward:<br>
```vb
' Suppose margin "top_row" is a RowMargin currently at TopPanel, row 1.
MarginPlusOne("top_row")
' Result: Moves "top_row" to row 2 (or wraps to Adjacent panel if out of bounds).
```

Move a Column Margin forward:<br>
```vb
' Suppose margin "right_col" is a ColumnMargin currently at RightPanel, column 0.
MarginPlusOne("right_col")
' Result: Moves "right_col" to column 1 (or wraps to Adjacent panel if out of bounds).
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`MarginJump`**

For moving margins (row or column) between panels and to specific positions.

&nbsp;&nbsp;Signature<br>
```vb
Sub MarginJump(
    marginId As String,
    newPanel As PanelType,
    newRow As Integer?,
    newCol As Integer?
)
```

&nbsp;&nbsp;API Enum: PanelType<br>
```vb
Public Enum PanelType
    BottomPanel
    NorthPanel
    EastPanel
    SouthPanel
    WestPanel
    TopPanel
End Enum
```

&nbsp;&nbsp;Usage<br>
For row margins (`Margin.Type = RowMargin`):<br>
&nbsp;&nbsp;&nbsp;&nbsp;newRow : Must be the target row index.<br>
&nbsp;&nbsp;&nbsp;&nbsp;newCol : Must be `Nothing`.<br>
For column margins (`Margin.Type = ColumnMargin`):<br>
&nbsp;&nbsp;&nbsp;&nbsp;newCol : Must be the target column index.<br>
&nbsp;&nbsp;&nbsp;&nbsp;newRow : Must be `Nothing`.<br>
The margin **must not** be locked; locked margins cannot be moved.

If you supply both `newRow` and `newCol`, or neither, an exception will be thrown.  

&nbsp;&nbsp;Example<br>
Move a Row Margin:<br>
```vb
' Move row margin "margin_row_1" to row 3 in the EastPanel
MarginJump(
    marginId:="margin_row_1",
    newPanel:=PanelType.EastPanel,
    newRow:=3,
    newCol:=Nothing
)
```

Move a Column Margin:<br>
```vb
' Move column margin "margin_col_99" to column 2 in the SouthPanel
MarginJump(
    marginId:="margin_col_99",
    newPanel:=PanelType.SouthPanel,
    newRow:=Nothing,
    newCol:=2
)
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Property<br>
**`objectDictionary`**

&nbsp;&nbsp;Signature<br>
```vb
ReadOnly Property objectDictionary As ConcurrentDictionary(Of Integer, MyObject)
```

&nbsp;&nbsp;Purpose<br>
Central registry of all `MyObject` instances in the host. 

&nbsp;&nbsp;Value type<br>
```vb
ConcurrentDictionary(Of Integer, MyObject)
```
&nbsp;&nbsp;&nbsp;&nbsp;Key:    object ID (Integer)<br>
&nbsp;&nbsp;&nbsp;&nbsp;Value:  MyObject instance

Populated via `api.AddMyObjectToFactory(x, y, z, structureId)` which returns the object's key.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`RemoveAllTrianglesInSet`**

&nbsp;&nbsp;Signature<br>
```vb
Sub RemoveAllTrianglesInSet(setId As Integer)
```

&nbsp;&nbsp;Purpose<br>
Removes all triangles belonging to the specified `setId` from the host's collision/occlusion system.

&nbsp;&nbsp;Plugin use cases <br>
Clear old triangles before adding updated geometry for a new frame.<br>
Remove an object's collision mesh when the object is destroyed or hidden. 

&nbsp;&nbsp;Plugin usage pattern<br>
```vb
api.RemoveAllTrianglesInSet(mySetId)
```

&nbsp;&nbsp;Notes<br>
Safe to call even if `setId` has no triangles (no-op).<br>
Always call before `AddTriangle` when updating geometry each frame.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`RemoveMargin`**

&nbsp;&nbsp;Signature<br>
```vb
Sub RemoveMargin(marginId As String)
```

Removes the margin identified by `marginId` from the host application.

Removes the specified margin** from the manager if it exists and is **not locked**.

&nbsp;&nbsp;Usage Example<br>
```vb
api.RemoveMargin("TopRow")
```

Removing a margin also disables its associated structure's visibility, unless locked.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`RemoveObjectsByStructureId`**

&nbsp;&nbsp;Signature<br>
```vb
Sub RemoveObjectsByStructureId(structureId As Integer)
```

&nbsp;&nbsp;Purpose<br>
Removes all objects associated with a given `structureId` from the host.<br>
Cleans up the object dictionary, structure-to-object mappings, and draw state.

&nbsp;&nbsp;What it clears<br>
All object IDs in `structureObjectIDs(structureId)` are removed from `objectDictionary`.

&nbsp;&nbsp;Plugin use cases<br>
Clean up all visual objects for a structure when unloading or resetting.<br>
Remove an entire object group before rebuilding it with new geometry. 

&nbsp;&nbsp;Plugin usage pattern<br>
Remove all objects for a structure:<br>
```vb
api.RemoveObjectsByStructureId(myStructureId)
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`SetMarginLock`**

&nbsp;&nbsp;Signature<br>
```vb
Sub SetMarginLock(marginId As String, lockState As Boolean)
```

&nbsp;&nbsp;Purpose<br>
Locks or unlocks a margin, controlling whether it is moveable or removable.

&nbsp;&nbsp;Usage<br>
Lock a Margin (prevent move/removal):<br>
```vb
api.SetMarginLock("myMarginId", True)
```

Unlock a Margin (allow move/removal):<br>
```vb
api.SetMarginLock("myMarginId", False)
```

When locked, margins are protected from being moved or deleted via UI or API.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`SetStructureDrawState`**

&nbsp;&nbsp;Signature<br>
```vb
Sub SetStructureDrawState(structureId As Integer, isOn As Boolean)
```

&nbsp;&nbsp;Purpose<br>
Controls whether objects belonging to a `structureId` are drawn by the host.<br>
Toggle visibility on/off without removing objects from the registry.

&nbsp;&nbsp;Plugin use cases<br>
Hide a structure temporarily (e.g., during transitions or when off-screen).<br>
Show a structure when it becomes active or enters view.<br>
Toggle debug/overlay geometry on and off.

&nbsp;&nbsp;Plugin usage patterns<br>
Enable drawing for a structure:<br>
```vb
api.SetStructureDrawState(myStructureId, True)
```

Disable drawing for a structure:<br>
```vb
api. SetStructureDrawState(myStructureId, False)
```

&nbsp;&nbsp;Notes<br>
Default state is `True` (visible) when objects are first added via `AddMyObjectToFactory`.<br>
Does not add or remove objects; only affects rendering.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Property<br>
**`structureObjectIDs`**

&nbsp;&nbsp;Signature<br>
```vb
ReadOnly Property structureObjectIDs As ConcurrentDictionary(Of Integer, ImmutableList(Of Integer))
```

&nbsp;&nbsp;Purpose<br>
Maps each `structureId` to an immutable list of `objectId` values belonging to that structure.<br>
Allows plugins to query which objects belong to a specific structure.

&nbsp;&nbsp;Value type<br>
```vb
ConcurrentDictionary(Of Integer, ImmutableList(Of Integer))
```
&nbsp;&nbsp;&nbsp;&nbsp;Key:    structureId (Integer)
&nbsp;&nbsp;&nbsp;&nbsp;Value:  ImmutableList of objectIds (Integer)

&nbsp;&nbsp;Plugin access patterns<br>
Check if a structure has objects:<br>
```vb
Dim objIds As ImmutableList(Of Integer) = Nothing
If api.structureObjectIDs.TryGetValue(myStructureId, objIds) Then
    ' Structure exists and has objects
End If
```

Iterate all object IDs in a structure:<br>
```vb
Dim objIds As ImmutableList(Of Integer) = Nothing
If api.structureObjectIDs.TryGetValue(myStructureId, objIds) Then
    For i As Integer = 0 To objIds.Count - 1
        Dim objId As Integer = objIds(i)
        ' Use objId with api.objectDictionary to get/modify object
    Next
End If
```

Get object data via cross-reference:<br>
```vb
Dim objIds As ImmutableList(Of Integer) = Nothing
If api.structureObjectIDs.TryGetValue(myStructureId, objIds) Then
    For i As Integer = 0 To objIds.Count - 1
        Dim obj As MyObject = Nothing
        If api.objectDictionary.TryGetValue(objIds(i), obj) Then
            ' Access obj.UpdateLocation(), obj.ColorOverride, etc.
        End If
    Next
End If
```

Treat as read-only; do not add/remove directly.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`SwapZoneMarginSets`**

&nbsp;&nbsp;Overview<br>
Toggles the active margin set for a spatial zone between two assigned sets-A and B.<br>
Use this when building plugins that need to "flip" zone layouts.

&nbsp;&nbsp;Concept<br>
Each spatial zone manages two "slots" for margin sets:<br>
&nbsp;&nbsp;&nbsp;&nbsp;MarginSetA** (often for "primary" layout)<br>
&nbsp;&nbsp;&nbsp;&nbsp;MarginSetB** (for "alternate" layout)

Calling `SwapZoneMarginSets(zoneId)` toggles which set is currently active in the specified zone.

&nbsp;&nbsp;Usage<br>
Swap active margin set in zone "zone1":<br>
```vb
api.SwapZoneMarginSets("zone1")
```

If currently showing A, toggles to B.<br>
If currently showing B, toggles to A.<br>
Does **not** change which set names are assigned to A/B; only switches between them.

&nbsp;&nbsp;Swapping after removal<br>
Using swapping methods on disposed zone references is possible but risky (unmanaged state, leaks).<br>
Avoid unless you're debugging or experimenting.


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method<br>
**`AssignZoneMarginSetA`**<br>
**`AssignZoneMarginSetB`**

&nbsp;&nbsp;Concept<br>
Each spatial zone manages two "slots" for margin sets:<br>
&nbsp;&nbsp;&nbsp;&nbsp;MarginSetA** (often for "primary" layout)<br>
&nbsp;&nbsp;&nbsp;&nbsp;MarginSetB** (for "alternate" layout)

These calls will *always* switch the zone to show the assigned MarginSetA or MarginSetB, regardless of current state.<br>
Use these explicit switches to always control which zone margin set is shown.<br>
They are your direct route for "show this layout now".

&nbsp;&nbsp;Common Usage Patterns<br>
Assign sets, then force a switch:<br>
```vb
api.AssignZoneMarginSetA("zone1", "SetA")
api.AssignZoneMarginSetB("zone1", "SetB")

api.SwitchZoneToMarginSetA("zone1")  ' zone1 now displays margins from "SetA"
api.SwitchZoneToMarginSetB("zone1")  ' zone1 now displays margins from "SetB"
```

If you want a simple toggle between two layouts, see `SwapZoneMarginSets(zoneId)` 

Switching is *stateless*: the slot assignment does **not** change. The displayed margins update according to the current slot.

If zones are removed via `RemoveSpatialZone(zoneId)`, any plugin references to the zone object may still work (but the host won't track them). Avoid manipulating "zombie" zones unless truly necessary.WIP


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Method  ** WIP<br>
**`ToggleMarginVisibility`**
Sub ToggleMarginVisibility(marginId As String)
 
Call
ToggleMarginVisibility(marginId As String)

Effect
Toggles visibility of specified margin

Inputs
A margin name string (e.g., `"TopRow"`, `"LeftColumn"`)

Usage Example
PluginApi.ToggleMarginVisibility("TopRow")


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Property<br>
**`triangleGroups`**

&nbsp;&nbsp;Signature<br>
```vb
ReadOnly Property triangleGroups As Object   ' actually: ConcurrentDictionary(Of Integer, ConcurrentBag(Of Integer))
```

&nbsp;&nbsp;Purpose<br>
Maps each `setId` to a bag of `triangleId` values belonging to that set.<br>
Allows plugins to query which triangles belong to a specific set.

&nbsp;&nbsp;Value type (implementation)<br>
```vb
ConcurrentDictionary(Of Integer, ConcurrentBag(Of Integer))
```
&nbsp;&nbsp;&nbsp;&nbsp;Key:    setId (Integer)<br>
&nbsp;&nbsp;&nbsp;&nbsp;Value:  bag of triangleIds (Integer)

&nbsp;&nbsp;Plugin use cases<br>
Check if a set exists (has any triangles registered).<br>
Retrieve all triangle IDs for a given set.<br>
Cross-reference with `trianglesById` to get full triangle geometry.

&nbsp;&nbsp;Plugin access patterns<br>
Check if a set has triangles:<br>
```vb
Dim triangleIds As ConcurrentBag(Of Integer) = Nothing
If api.triangleGroups.TryGetValue(mySetId, triangleIds) Then
    ' Set exists and has triangles
End If
```

Iterate all triangle IDs in a set:<br>
```vb
Dim triangleIds As ConcurrentBag(Of Integer) = Nothing
If api.triangleGroups.TryGetValue(mySetId, triangleIds) Then
    For Each triangleId As Integer In triangleIds
        ' Use triangleId with api.trianglesById to get geometry
    Next
End If
```

Get triangle geometry via cross-reference:<br>
```vb
Dim triangleIds As ConcurrentBag(Of Integer) = Nothing
If api.triangleGroups.TryGetValue(mySetId, triangleIds) Then
    For Each triangleId As Integer In triangleIds
        Dim tri As Triangle
        If api.trianglesById.TryGetValue(triangleId, tri) Then
            ' Access tri.A, tri.B, tri.C for vertex positions
        End If
    Next
End If
```


<br><br><br><br>


===<br>
&nbsp;&nbsp;API Property<br>
**`trianglesById`**

&nbsp;&nbsp;Signature<br>
```vb
ReadOnly Property trianglesById As Object   ' actually: ConcurrentDictionary(Of Integer, Triangle)
```

&nbsp;&nbsp;Purpose<br>
Exposes the host's triangle registry for ray-occlusion / collision tests.<br>
Dictionary is keyed by `TriangleId`; values are `Triangle` records.

&nbsp;&nbsp;Triangle layout<br>
```vb
Structure Triangle
    Public A As (Double, Double, Double)
    Public B As (Double, Double, Double)
    Public C As (Double, Double, Double)
    Public TriangleSetId As Integer
    Public TriangleId As Integer
End Structure
```

&nbsp;&nbsp;Typical access pattern (read-only)<br>
```vb
Dim tri As Triangle
If api.trianglesById.TryGetValue(triangleId, tri) Then
    ' Use tri.A, tri.B, tri.C for geometry or ray tests
End If
```


<br><br><br><br>


===
