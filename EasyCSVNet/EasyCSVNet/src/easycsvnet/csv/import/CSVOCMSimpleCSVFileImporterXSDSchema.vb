#Region "Options"

Option Strict On
Option Explicit On

#End Region 'Options

Namespace easycsvnet
    Namespace csv
        Namespace import

            Public Class CSVOCMSimpleCSVFileImporterXSDSchema

                Public Shared XML As String = <?xml version="1.0" encoding="UTF-8"?>
                                              <!--
                	XML/XSD Schema for Object-CSV mapping (OCM) definition
                	@author alex.debuen@gmail.com
                	@version 1.0.0.0 (20170824_150000)
                -->
                                              <xsd:schema targetNamespace="urn:csv"
                                                  xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                                                  xmlns:csv="urn:csv">

                                                  <!-- SimpleCSVImporter :: XSD definition schema [v.1.0.0.0 / 2017-08-24 15:00:00h] -->

                                                  <!-- Simple types -->

                                                  <xsd:simpleType name="chartype">
                                                      <xsd:restriction base="xsd:string">
                                                          <xsd:pattern value="[a-zA-Z0-9_#:;,.@%\(\)\[\]\{\}\|$]{1}"/>
                                                      </xsd:restriction>
                                                  </xsd:simpleType>

                                                  <xsd:simpleType name="idtype">
                                                      <xsd:restriction base="xsd:string">
                                                          <xsd:pattern value="[a-zA-Z0-9_]+"/>
                                                      </xsd:restriction>
                                                  </xsd:simpleType>

                                                  <!-- Main structure -->

                                                  <xsd:element name="csv">
                                                      <xsd:complexType>
                                                          <xsd:sequence>
                                                              <xsd:element name="metadata">
                                                                  <xsd:complexType>
                                                                      <xsd:sequence>
                                                                          <!-- File author [optional] -->
                                                                          <xsd:element name="author" type="xsd:string" minOccurs="0" maxOccurs="1"/>
                                                                          <!-- File version -->
                                                                          <xsd:element name="version" type="xsd:string" minOccurs="1" maxOccurs="1"/>
                                                                          <!-- File build/revision (datetime formatted as 'YYYY-MM-DD[+hh:mm]') [optional]-->
                                                                          <xsd:element name="revision" type="xsd:date" minOccurs="0" maxOccurs="1"/>
                                                                          <!-- FQDN of the mapped object (DTO) -->
                                                                          <xsd:element name="mappedobjectfqdn" type="xsd:string" minOccurs="1" maxOccurs="1"/>
                                                                          <!-- File description [optional] -->
                                                                          <xsd:element name="description" type="xsd:string" minOccurs="0" maxOccurs="1"/>
                                                                      </xsd:sequence>
                                                                  </xsd:complexType>
                                                              </xsd:element><!-- /metadata -->
                                                              <xsd:element name="data">
                                                                  <xsd:complexType>
                                                                      <xsd:sequence>
                                                                          <xsd:element name="settings">
                                                                              <xsd:complexType>
                                                                                  <xsd:sequence>
                                                                                      <!-- Should ommit first line (considered as a title or header line)? {true|false} -->
                                                                                      <xsd:element name="titles" type="xsd:boolean" minOccurs="1" maxOccurs="1"/>
                                                                                      <!-- Field separator character -->
                                                                                      <xsd:element name="delimiter" type="csv:chartype" minOccurs="1" maxOccurs="unbounded"/>
                                                                                      <!-- Should strictly check field types while parsing/marshalling? {true|false} -->
                                                                                      <xsd:element name="stricttypecast" type="xsd:boolean" minOccurs="1" maxOccurs="1"/>
                                                                                      <!-- Encoding (VB.Net FQDN System.Text.Encoding.{ASCII|BigEndianUnicode|Default|Unicode|UTF7|UTF8|UTF32}) [optional] -->
                                                                                      <xsd:element name="encoding" type="xsd:string" minOccurs="0" maxOccurs="1"/>
                                                                                  </xsd:sequence>
                                                                              </xsd:complexType>
                                                                          </xsd:element><!-- /settings -->
                                                                          <xsd:element name="fields">
                                                                              <xsd:complexType>
                                                                                  <xsd:choice minOccurs="1" maxOccurs="unbounded">
                                                                                      <xsd:element name="field">
                                                                                          <xsd:complexType>
                                                                                              <!-- Field ID -->
                                                                                              <xsd:attribute name="id" type="csv:idtype" use="required"/>
                                                                                              <!-- Field position within CSV line [0...*] -->
                                                                                              <xsd:attribute name="position" type="xsd:int" use="required"/>
                                                                                              <!-- Empty/blank value allowed? {true|false} -->
                                                                                              <xsd:attribute name="nillable" type="xsd:boolean" use="required"/>
                                                                                              <!-- Quotation marks surrounding field value allowed? {true|false} -->
                                                                                              <xsd:attribute name="quoted" type="xsd:boolean" use="required"/>
                                                                                              <!-- Field descriptional [optional] -->
                                                                                              <xsd:attribute name="description" type="xsd:string" use="optional"/>
                                                                                          </xsd:complexType>
                                                                                      </xsd:element><!-- /field -->
                                                                                  </xsd:choice>
                                                                              </xsd:complexType>
                                                                          </xsd:element><!-- /fields -->
                                                                          <xsd:element name="mapping">
                                                                              <xsd:complexType>
                                                                                  <xsd:choice minOccurs="1" maxOccurs="unbounded">
                                                                                      <xsd:element name="map">
                                                                                          <xsd:complexType>
                                                                                              <!-- Field ID (FK for <csv:field>.id) -->
                                                                                              <xsd:attribute name="field" type="csv:idtype" use="required"/>
                                                                                              <!-- Attribute or property of the mapped DTO object this field will be marshalled into -->
                                                                                              <xsd:attribute name="attribute" type="csv:idtype" use="required"/>
                                                                                              <!-- Mapping rule description [optional] -->
                                                                                              <xsd:attribute name="description" type="xsd:string" use="optional"/>
                                                                                          </xsd:complexType>
                                                                                      </xsd:element><!-- /map -->
                                                                                  </xsd:choice>
                                                                              </xsd:complexType>
                                                                          </xsd:element><!-- /mapping -->
                                                                      </xsd:sequence>
                                                                  </xsd:complexType>
                                                              </xsd:element><!-- /data -->
                                                          </xsd:sequence>
                                                      </xsd:complexType>
                                                  </xsd:element><!-- /csv -->

                                              </xsd:schema>.ToString()

                Private Sub New()
                    ' Static class
                End Sub

            End Class

        End Namespace
    End Namespace
End Namespace
