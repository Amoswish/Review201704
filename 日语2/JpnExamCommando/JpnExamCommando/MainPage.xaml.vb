﻿' https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

Imports Nukepayload2.VisualBasicExtensions.UWP
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Windows.Media.Core
Imports Windows.Media.Playback
Imports Windows.Media.SpeechSynthesis
Imports Windows.Storage

''' <summary>
''' 可用于自身或导航至 Frame 内部的空白页。
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Dim _getCourse As New Regex("(?<=\w+)\d{2}(?=\.md)")

    Public Property DataSource As VocabularyGroup()
        Get
            Return GetValue(DataSourceProperty)
        End Get
        Set
            SetValue(DataSourceProperty, Value)
        End Set
    End Property
    Public Shared ReadOnly DataSourceProperty As DependencyProperty =
                           DependencyProperty.Register(NameOf(DataSource),
                           GetType(VocabularyGroup()), GetType(MainPage),
                           New PropertyMetadata(Nothing))

    Private Async Sub MainPage_LoadedAsync(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim needToGenerate = Await LoadDataAsync()
        InkTraining.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Touch Or Windows.UI.Core.CoreInputDeviceTypes.Pen Or Windows.UI.Core.CoreInputDeviceTypes.Mouse
        If needToGenerate Then
            Dim json = Await FileIO.ReadTextAsync(Await StorageFile.GetFileFromApplicationUriAsync(New Uri("ms-appx:///vocabulary8to20.json")))
            Dim courseView = JsonConvert.DeserializeObject(Of VocabularyCourse())(json)
            Dim groupItems As New List(Of VocabularyGroupItem)
            For Each course In courseView
                Dim courseId = CInt(_getCourse.Match(course.md).Value)
                groupItems.AddRange(From itm In course.Content
                                    Select New VocabularyGroupItem(itm.假名, itm.日文汉字, itm.翻译, itm.词性, courseId))
            Next
            Dim grouped = Aggregate itm In groupItems Group By itm.Kind Into Group
                          Select New VocabularyGroup With {.Kind = Kind, .Group = Group.ToArray} Into ToArray
            DataSource = grouped
        End If
    End Sub

    Private Async Function LoadDataAsync() As Task(Of Boolean)
        Dim needToGenerate = False
        Dim dataFolder = ApplicationData.Current.LocalFolder
        Dim cachedDataSource = Await dataFolder.CreateFileAsync("CachedDataSource.json", CreationCollisionOption.OpenIfExists)
        Using strm = Await cachedDataSource.OpenStreamForReadAsync
            If strm.Length = 0 Then
                needToGenerate = True
            Else
                Try
                    Dim text = Await New StreamReader(strm).ReadToEndAsync
                    DataSource = JsonConvert.DeserializeObject(Of VocabularyGroup())(text)
                Catch ex As Exception
                    needToGenerate = True
                End Try
            End If
        End Using
        Return needToGenerate
    End Function

    Dim saving As Boolean
    Private Async Function SaveDataAsync() As Task
        Do While saving
            Await Task.Delay(1)
        Loop
        saving = True
        Dim errorOccured = String.Empty
        Try
            Dim dataFolder = ApplicationData.Current.LocalFolder
            Dim cachedDataSource = Await dataFolder.CreateFileAsync("CachedDataSource.json", CreationCollisionOption.OpenIfExists)
            Dim text = JsonConvert.SerializeObject(DataSource)
            Await FileIO.WriteTextAsync(cachedDataSource, text)
        Catch ex As Exception
            errorOccured = ex.Message
        End Try
        saving = False
        If Not String.IsNullOrEmpty(errorOccured) Then
            Await MsgBoxAsync(errorOccured,, "写入失败")
        End If
    End Function

    Private Sub MainPage_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        SplTrain.OpenPaneLength = ActualWidth
    End Sub

    Private Sub BtnBack_Click(sender As Object, e As RoutedEventArgs) Handles BtnBack.Click
        SplTrain.IsPaneOpen = False
        InkTraining.InkPresenter.StrokeContainer.Clear()
    End Sub

    Private Sub BtnClearInk_Click(sender As Object, e As RoutedEventArgs) Handles BtnClearInk.Click
        InkTraining.InkPresenter.StrokeContainer.Clear()
    End Sub

    Private Sub GridView_ItemClick(sender As Object, e As ItemClickEventArgs)
        GrdSelectedItem.DataContext = e.ClickedItem
        SplTrain.IsPaneOpen = True
    End Sub

    Private Async Sub ToggleButton_Click(sender As Object, e As RoutedEventArgs)
        GrdWaiting.Visibility = Visibility.Visible
        Await SaveDataAsync()
        GrdWaiting.Visibility = Visibility.Collapsed
    End Sub

    Dim _speechJp As New SpeechSynthesizer
    Dim _speechCn As New SpeechSynthesizer

    Private Async Sub BtnPlay_Click(sender As Object, e As RoutedEventArgs)
        BtnLoopPlay.IsEnabled = False
        Await PlayAsync()
        BtnLoopPlay.IsEnabled = True
    End Sub

    Private Async Sub BtnPlayLoop_Click(sender As Object, e As RoutedEventArgs)
        Do While BtnLoopPlay.IsChecked AndAlso BtnPlay.IsEnabled
            Await PlayAsync()
        Loop
    End Sub

    Private Async Function PlayAsync() As Task
        If Not BtnPlay.IsEnabled Then Return
        BtnPlay.IsEnabled = False
        Try
            Dim voiceJp = From v In SpeechSynthesizer.AllVoices Where v.Language = "ja-JP"
            If Not voiceJp.Any Then
                Await MsgBoxAsync("请先安装日语语言包和日语语音。",, "缺少语音包")
                Return
            Else
                _speechJp.Voice = voiceJp.First
            End If
            Dim jm$ = BtnPlay.Tag.Jm
            Dim cn$ = BtnPlay.Tag.Translation
            Dim strm = Await _speechJp.SynthesizeTextToStreamAsync(jm)
            Dim cnStrm = Await _speechCn.SynthesizeTextToStreamAsync(cn)
            Dim ja = MediaSource.CreateFromStream(strm, strm.ContentType)
            Dim waiting = True
            Dim handler As TypedEventHandler(Of MediaPlayer, System.Object) =
                Async Sub(s, arg)
                    Await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    Sub()
                        RemoveHandler wmp.MediaPlayer.MediaEnded, handler
                        Dim cnHandler As TypedEventHandler(Of MediaPlayer, System.Object) =
                            Async Sub(s2, e2)
                                Await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                Sub()
                                    RemoveHandler wmp.MediaPlayer.MediaEnded, cnHandler
                                    waiting = False
                                End Sub)
                            End Sub
                        AddHandler wmp.MediaPlayer.MediaEnded, cnHandler
                        wmp.Source = MediaSource.CreateFromStream(cnStrm, cnStrm.ContentType)
                    End Sub)
                End Sub
            AddHandler wmp.MediaPlayer.MediaEnded, handler
            wmp.Source = ja
            Do While waiting
                Await Task.Delay(1)
            Loop
        Finally
            BtnPlay.IsEnabled = True
        End Try
    End Function
End Class
