﻿Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Xml

Public Class ImportTool


    Private Sub ImportTool_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ''Dim dataGridView1 As New DataGridView()

        '' Adicionar colunas ao controle DataGridView
        'DataGridView1.Columns.Add("Coluna1", "Texto")
        'Dim coluna2 As New DataGridViewComboBoxColumn()
        'coluna2.HeaderText = "ComboBox"
        'coluna2.Name = "ComboBox"
        'Dim cell As New DataGridViewComboBoxCell()
        'cell.Items.Add("Opção 1")
        'cell.Items.Add("Opção 2")
        'cell.Items.Add("Opção 3")
        'coluna2.CellTemplate = cell
        'DataGridView1.Columns.Add(coluna2)

        '' Adicionar linhas ao controle DataGridView
        'DataGridView1.Rows.Add("Valor 1", "Opção 1")
        'DataGridView1.Rows.Add("Valor 2", "Opção 2")
        'DataGridView1.Rows.Add("Valor 3", "Opção 3")


    End Sub

    Private Sub AddFraisaTool()


        Dim splitLine() As String = My.Resources.DIN400_tool_params.Split(New String() {Environment.NewLine}, StringSplitOptions.None).ToArray

        Dim paramToPropDict As New Dictionary(Of String, String)
        For Each line As String In splitLine
            Dim fields() As String = line.Split(";"c)
            If fields.Count > 2 Then
                paramToPropDict.Add(fields(0), fields(1))

            End If
        Next

        Dim xmlFile As String = GetFile()

        Dim documentoXml As New XmlDocument()

        documentoXml.Load(xmlFile)

        Dim xmlDoc As XmlElement = documentoXml.DocumentElement

        Dim toolNode As XmlNode = xmlDoc.SelectSingleNode("//Tool")

        ' Cria uma nova instância da classe NewTool
        Dim newTool As New NewTool()

        For Each node As XmlNode In toolNode.ChildNodes
            ' Aqui você pode percorrer todos os parâmetros de cada toolNode
            For Each paramNode As XmlNode In node.SelectNodes("Property-Data")
                Dim paramName As String = paramNode.SelectSingleNode("PropertyName").InnerText.Trim()
                Dim paramValue As String = paramNode.SelectSingleNode("Value").InnerText.Trim()

                ' Verifica se o nome do parâmetro existe no dicionário
                If paramToPropDict.ContainsKey(paramName) Then
                    Dim propName As String = paramToPropDict(paramName)

                    ' Usa reflection para definir o valor da propriedade correspondente na classe NewTool
                    Dim prop As PropertyInfo = GetType(NewTool).GetProperty(propName)
                    'Correcting name
                    If IsNumeric(paramValue) Then
                        paramValue = paramValue.Replace(",", ".")
                    End If
                    If (paramValue = "FSA") Then paramValue = "FRAISA" 'TODO check list to show right name
                    prop.SetValue(newTool, Convert.ChangeType(paramValue, prop.PropertyType), Nothing)
                End If
            Next
        Next
        Set_Name_auto(newTool)
        newTool.Name = Main.Name_textbox.Text

        FillDataGrid(newTool, DataGridView1)

        Debug.WriteLine(newTool)
    End Sub

    Private Sub find_Bt_Click(sender As Object, e As EventArgs) Handles find_Bt.Click
        AddFraisaTool()
    End Sub

    Private Function GetFile() As String
        Dim itemCode As String = RefTextBox.Text

        itemCode = itemCode.Replace(",", "")
        itemCode = itemCode.Replace(".", "")

        If itemCode <> "" Then
            Dim url As String = "https://fsa.salessupportserver.com/CIMDataService_3S-FSA/DownloadService.svc/web/GetExport?OrderCode=" + itemCode + "&ExportType=din4000xml2016"
            Dim nomeArquivo As String = itemCode + ".xml"

            Try
                Dim cliente As New WebClient()
                Dim resposta As String = cliente.DownloadString(url)
                If resposta = "Exception occurred - please contact support" Then
                    outputLabel.Text = "Tool not found"
                    Return ""
                End If
                File.WriteAllText(nomeArquivo, resposta)
                outputLabel.Text = "Tool found!"
                Return nomeArquivo
            Catch ex As Exception
                outputLabel.Text = "Error downloading tool: " + ex.Message
                Return ""
            End Try
        Else
            outputLabel.Text = "Fill tool reference"
            Return ""
        End If
        outputLabel.Visible = True
    End Function



End Class