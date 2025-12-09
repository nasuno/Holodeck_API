
Imports System.Collections.Concurrent
Imports System.Collections.Immutable

' ============================================
' ==========  SHARED ENUMS  ==================
' ============================================


Public Enum ObjectColor ' (Use = Nothing for no override) 
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

Public Enum PanelType
    BottomPanel
    NorthPanel
    EastPanel
    SouthPanel
    WestPanel
    TopPanel
End Enum

Public Enum PanelCorner
    Bottom_a
    Bottom_b
    Bottom_c
    Bottom_d
    North_a
    North_b
    North_c
    North_d
    East_a
    East_b
    East_c
    East_d
    South_a
    South_b
    South_c
    South_d
    West_a
    West_b
    West_c
    West_d
    Top_a
    Top_b
    Top_c
    Top_d
End Enum

Public Enum MarginType
    RowMargin
    ColumnMargin
End Enum

' ============================================
' ==========  SHARED DATA CLASSES  ===========
' ============================================

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

' ============================================
' ==========  MAIN HOST API INTERFACE  =======
' ============================================

Public Interface ICurrentApi

    ' === OBJECT CONTROL ===
    Sub RemoveObjectsByStructureId(structureId As Integer)
    Function GetNextUniqId() As Integer
    Function GetMyObjectByStructureId(structureId As Integer) As MyObject
    ReadOnly Property objectDictionary As ConcurrentDictionary(Of Integer, MyObject)
    ReadOnly Property structureObjectIDs As ConcurrentDictionary(Of Integer, ImmutableList(Of Integer))
    Sub SetStructureDrawState(structureId As Integer, isOn As Boolean)

    ' === UTILITY ===
    Function Bresenham3D(startX As Integer, startY As Integer, startZ As Integer,
                         endX As Integer, endY As Integer, endZ As Integer) As List(Of (Integer, Integer, Integer))
    Sub ThinEvenSpatiallyAdaptiveAuto(
        ByRef sourceDict As ConcurrentDictionary(Of Integer, MyObject),
        ByRef destDict As ConcurrentDictionary(Of Integer, MyObject),
        numToLeave As Integer,
        observer As (Integer, Integer, Integer),
        keepRadius As Double,
        Optional numBands As Integer = 10,
        Optional closeBiasExponent As Double = 1.5)

    ' === TRIANGLE CONTROL ===
    Sub RemoveAllTrianglesInSet(setId As Integer)
    Function AddTriangle(x1 As Double, y1 As Double, z1 As Double,
                        x2 As Double, y2 As Double, z2 As Double,
                        x3 As Double, y3 As Double, z3 As Double,
                        setId As Integer) As Integer
    ReadOnly Property triangleGroups As Object   ' <- Use actual type if available
    ReadOnly Property trianglesById As Object    ' <- Use actual type if available

    ' === OBJECT FACTORY ===
    Function AddMyObjectToFactory(x As Integer, y As Integer, z As Integer, structureId As Integer) As Integer

    ' === MARGIN CONTROL ===
    Sub CreateMargin(marginId As String, marginType As MarginType, panel As PanelType, row As Integer?, column As Integer?, locked As Boolean)
    Sub MarginJump(marginId As String, newPanel As PanelType, newRow As Integer?, newCol As Integer?)
    Sub MarginPlusOne(marginId As String)
    Sub MarginMinusOne(marginId As String)
    Sub SetMarginLock(marginId As String, lockState As Boolean)
    Sub RemoveMargin(marginId As String)
    Sub ToggleMarginVisibility(marginId As String)






    ' === SPATIAL ZONE CONTROL ===
    Function CreateSpatialZone(zoneId As String) As ISpatialZone
    Sub RemoveSpatialZone(zoneId As String)   ' *** WIP    DO NOT USE
    Function GetSpatialZone(zoneId As String) As ISpatialZone

    Function GetAllMarginIDs() As Dictionary(Of String, List(Of String))
    Function GetMarginInfoSnapshot(marginId As String) As Dictionary(Of String, Object)

    ' === MARGIN SET APIs ===
    Sub CreateMarginSet(setName As String,
                    topRowMarginId As String,
                    bottomRowMarginId As String,
                    leftColumnMarginId As String,
                    rightColumnMarginId As String)

    Function GetAllMarginSetNames() As List(Of String)
    Function GetMarginSet(setName As String) As Dictionary(Of String, String)

    ' Query currently active set for a zone
    Function GetZoneAssignedMarginSet(zoneId As String) As String

    ' === ZONE MARGIN SET A/B CONTROL ===

    ' Assign logical “slot A” and “slot B” for a zone
    Sub AssignZoneMarginSetA(zoneId As String, setName As String)
    Sub AssignZoneMarginSetB(zoneId As String, setName As String)

    ' Force zone to display a particular slot, regardless of current state
    Sub SwitchZoneToMarginSetA(zoneId As String)
    Sub SwitchZoneToMarginSetB(zoneId As String)

    ' Swap: toggle between marginSetA and marginSetB
    Sub SwapZoneMarginSets(zoneId As String)

    ' === PANEL BOUNDS ===
    Function GetPanelFurthestLeftColumn(panel As PanelType) As Integer
    Function GetPanelFurthestTopRow(panel As PanelType) As Integer
    Function GetPanelFurthestRightColumn(panel As PanelType) As Integer
    Function GetPanelFurthestBottomRow(panel As PanelType) As Integer

    ' === ZONE LIST ===
    Function GetAllSpatialZones() As IEnumerable(Of ISpatialZone)

    ' === OBSERVER LOCATION ===
    Function GetObserverOrigin() As (Integer, Integer, Integer)
    Function GetObserverUnitVector() As (Double, Double, Double)
End Interface

' ============================================
' ==========  SPATIAL ZONE INTERFACE  ========
' ============================================

Public Interface ISpatialZone
    Sub UpdateMargins(leftId As String, rightId As String, topId As String, bottomId As String)
    Sub DisposeZone()
    ReadOnly Property Left As Integer
    ReadOnly Property Right As Integer
    ReadOnly Property Top As Integer
    ReadOnly Property Bottom As Integer
    Property Text As String
    ReadOnly Property ID As String

    ' Font grid
    Function GetAllFontSegments() As List(Of (Integer, Integer))
    Sub SetGutterVisible(row As Integer, col As Integer, side As String, visible As Boolean)

    ' Char mapping
    ReadOnly Property WrappedCharIndex As Dictionary(Of (Integer, Integer), Char)

    ' Bounding box
    ReadOnly Property BoundingBoxAABB As ((Integer, Integer, Integer), (Integer, Integer, Integer))

    ' Color inversion  ** DO NOT USE
    Sub InvertColorsOn()
    Sub InvertColorsOff()
End Interface

' ============================================
' ==========  PLUGIN LOCATOR UTILITY  ========
' ============================================

' A tiny global registry so plugins can publish and consume arbitrary objects by key.
Public NotInheritable Class PluginLocator
    Private Shared ReadOnly _services As New Dictionary(Of String, Object)

    ' Register an instance under a unique key
    Public Shared Sub Register(key As String, instance As Object)
        _services(key) = instance
    End Sub

    ' Try to retrieve and cast to T
    Public Shared Function [Get](Of T As Class)(key As String) As T
        Dim obj As Object = Nothing
        If _services.TryGetValue(key, obj) Then
            Return TryCast(obj, T)
        End If
        Return Nothing
    End Function
End Class
