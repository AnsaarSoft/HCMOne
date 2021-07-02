<?xml version="1.0" encoding="utf-8"?>
<root>
  <!-- 
    Microsoft ResX Schema 
    
    Version 2.0
    
    The primary goals of this format is to allow a simple XML format 
    that is mostly human readable. The generation and parsing of the 
    various data types are done through the TypeConverter classes 
    associated with the data types.
    
    Example:
    
    ... ado.net/XML headers & schema ...
    <resheader name="resmimetype">text/microsoft-resx</resheader>
    <resheader name="version">2.0</resheader>
    <resheader name="reader">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
    <resheader name="writer">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
    <data name="Name1"><value>this is my long string</value><comment>this is a comment</comment></data>
    <data name="Color1" type="System.Drawing.Color, System.Drawing">Blue</data>
    <data name="Bitmap1" mimetype="application/x-microsoft.net.object.binary.base64">
        <value>[base64 mime encoded serialized .NET Framework object]</value>
    </data>
    <data name="Icon1" type="System.Drawing.Icon, System.Drawing" mimetype="application/x-microsoft.net.object.bytearray.base64">
        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
        <comment>This is a comment</comment>
    </data>
                
    There are any number of "resheader" rows that contain simple 
    name/value pairs.
    
    Each data row contains a name, and value. The row also contains a 
    type or mimetype. Type corresponds to a .NET class that support 
    text/value conversion through the TypeConverter architecture. 
    Classes that don't support this are serialized and stored with the 
    mimetype set.
    
    The mimetype is used for serialized objects, and tells the 
    ResXResourceReader how to depersist the object. This is currently not 
    extensible. For a given mimetype the value must be set accordingly:
    
    Note - application/x-microsoft.net.object.binary.base64 is the format 
    that the ResXResourceWriter will generate, however the reader can 
    read any of the formats listed below.
    
    mimetype: application/x-microsoft.net.object.binary.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            : and then encoded with base64 encoding.
    
    mimetype: application/x-microsoft.net.object.soap.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter
            : and then encoded with base64 encoding.

    mimetype: application/x-microsoft.net.object.bytearray.base64
    value   : The object must be serialized into a byte array 
            : using a System.ComponentModel.TypeConverter
            : and then encoded with base64 encoding.
    -->
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="ConError" xml:space="preserve">
    <value>Error in SBO connection.</value>
  </data>
  <data name="dbError_TCR" xml:space="preserve">
    <value>Error in table creation procedure.</value>
  </data>
  <data name="xception" xml:space="preserve">
    <value>Error :</value>
  </data>
  <data name="Nev_Rec_First" xml:space="preserve">
    <value>Reached to first record.</value>
    <comment>Ubaid</comment>
  </data>
  <data name="Nev_Rec_Last" xml:space="preserve">
    <value>Reached to last record.</value>
    <comment>Ubaid</comment>
  </data>
  <data name="String2" xml:space="preserve">
    <value />
  </data>
  <data name="Err_Required" xml:space="preserve">
    <value>Please provide the required data.</value>
    <comment>Ubaid</comment>
  </data>
  <data name="Err_EmpDID" xml:space="preserve">
    <value>Employee ID already exist.</value>
    <comment>Faisal</comment>
  </data>
  <data name="Inf_EmpID" xml:space="preserve">
    <value>Employee ID is mandatory for employee to save record.</value>
    <comment>Faisal</comment>
  </data>
  <data name="Err_DateComparison" xml:space="preserve">
    <value>End date must be greater than or equal to start date.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_DupRecord" xml:space="preserve">
    <value>You can't add duplicate records.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_ExistCode" xml:space="preserve">
    <value>Entered code already exist.</value>
    <comment>Salman</comment>
  </data>
  
  <data name="Err_ExistDesc" xml:space="preserve">
    <value>Entered description already exist.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_ExistDtRange" xml:space="preserve">
    <value>Specifed period or date range already exist.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_ExistEmployee" xml:space="preserve">
    <value>Employee already exist in database.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_ExistGroupID" xml:space="preserve">
    <value>Group ID already exist</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_ExistKeyObj" xml:space="preserve">
    <value>Key prerformance objective already exist</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_ExistName" xml:space="preserve">
    <value>Name already exist.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_ExistPoints" xml:space="preserve">
    <value>Entered points alreday exist.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_InvalidFormat" xml:space="preserve">
    <value>Invalid time format : Time format should be in 24hrs format. i.e. '23:59'.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_MandatoryFields" xml:space="preserve">
    <value>Select mandatory fields.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NoRecord" xml:space="preserve">
    <value>No record to display.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullAprslNo" xml:space="preserve">
    <value>Select appraisal No.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullAprsrCode" xml:space="preserve">
    <value>Select appraiser code.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullCity" xml:space="preserve">
    <value>City cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullCmptncyGrp" xml:space="preserve">
    <value>Select competency group.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullCode" xml:space="preserve">
    <value>Code cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullCountry" xml:space="preserve">
    <value>Country Cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullDate" xml:space="preserve">
    <value>Start Date ,End Date cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullDedCode" xml:space="preserve">
    <value>Deduction code cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullDesc" xml:space="preserve">
    <value>Description cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullLeaveCode" xml:space="preserve">
    <value>Leave code cannot be empty.</value>
    <comment>Salman</comment>
    
  </data>
  <data name="Err_NullLeaveType" xml:space="preserve">
    <value>Leave type cannot be empty.</value>
    <comment>Salman</comment>
    
  </data>
  <data name="Err_NullLeaveEnc" xml:space="preserve">
    <value>Deductable leave Type could not marked as leave encashment.</value>
    <comment>Salman</comment>    
  </data>
  <data name="Err_NullEncashActive" xml:space="preserve">
    <value>Leave encashment check box is mandatory for encashment type.</value>
    <comment>Salman</comment>    
  </data>
  <data name="Err_NullDocDate" xml:space="preserve">
    <value>Enter document date.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullEmployee" xml:space="preserve">
    <value>Please select employee.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullFrmWeek" xml:space="preserve">
    <value>Weekend from not selected.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullGroupID" xml:space="preserve">
    <value>Group id cannot be left empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullHol" xml:space="preserve">
    <value>Holiday name cannot be null.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullJobPos" xml:space="preserve">
    <value>Job postion cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullKeyObj" xml:space="preserve">
    <value>Key prformance objective cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullLagKPI" xml:space="preserve">
    <value>Laggigng KPI cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullLedKPI" xml:space="preserve">
    <value>Leading LPI cannot be empty.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullLevType" xml:space="preserve">
    <value>Leave type cannot be left empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullLinMngr" xml:space="preserve">
    <value>Select line manager.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullMinExp" xml:space="preserve">
    <value>Minimum experience cannot be empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullName" xml:space="preserve">
    <value>Name Cannot Be Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullOtTyp" xml:space="preserve">
    <value>Overtime Type Cannot Be Left Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullPlanNo" xml:space="preserve">
    <value>Select Performance Plan No.</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullPoints" xml:space="preserve">
    <value>Points Cannot be Left Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullPrntDesg" xml:space="preserve">
    <value>Parent Designation Cannot be Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullProfileStatus" xml:space="preserve">
    <value>Select Profile Status</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullSeries" xml:space="preserve">
    <value>Select Series</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullShiftEnd" xml:space="preserve">
    <value>Shift End Time Cannot Be Left Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullShiftST" xml:space="preserve">
    <value>Shift Start Time Cannot Be Left Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullShiftTyp" xml:space="preserve">
    <value>Shift Type Cannot Be Left Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullStatus" xml:space="preserve">
    <value>Status Cannot be Left Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullToWeek" xml:space="preserve">
    <value>Weekend To Not Selected</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullType" xml:space="preserve">
    <value>Deduction Type Cannot Be Left Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="Err_NullWorldRank" xml:space="preserve">
    <value>World Rank Cannot Be Empty</value>
    <comment>Salman</comment>
  </data>
  <data name="War_DelRow" xml:space="preserve">
    <value>This will Delete Record from Database, Do you want to Delete Record</value>
    <comment>Salman</comment>
  </data>
  <data name="War_SelectRow" xml:space="preserve">
    <value>No Row Selected, Please Select Row to Delete</value>
    <comment>Salman</comment>
  </data>
  <data name="ValidationFailed" xml:space="preserve">
    <value>Valdiation Failed</value>
    <comment>Ubaid</comment>
  </data>
  <data name="IncConfirm" xml:space="preserve">
  <value>Assigning Increment will update the salaries of current employee. Are you sure to assing the increment (Yes / No) </value>
  <comment>Ubaid</comment>
  </data>
  <data name="ConnectionSuccessfull" xml:space="preserve">
    <value>Connection with database established successfully.</value>
    <comment>Zeeshan</comment>
  </data>
  <data name="ConnectionFailed" xml:space="preserve">
    <value>Connection with database failed.</value>
    <comment>Zeeshan</comment>
  </data>
	<data name="Gen_ChangeSuccess" xml:space="preserve">
  <value>Record updated successfully!</value>
  <comment>Ubaid</comment>
  </data>
  <data name="INF_SelectCandidate" xml:space="preserve">
  <value>Select atleast one candidate.</value>
  <comment>MFM</comment>
  </data>
  <data name="INF_SelectEmployee" xml:space="preserve">
  <value>Select atleast one Employee.</value>
  <comment>Zeeshan</comment>
  </data>
  <data name="INF_AttendanceDates" xml:space="preserve">
  <value>Please enter valid dates to process attendance.</value>
  <comment>Zeeshan</comment>
  </data>
  <data name="RecordSavedSuccessfully" xml:space="preserve">
  <value>Record saved successfully.</value>
  <comment>Zeeshan</comment>
  </data>
	<data name="INF_SelectEmployee" xml:space="preserve">
  <value>Select atleast one employee.</value>
  <comment>Zeeshan</comment>
  </data>
	<data name="INF_AttendanceDates" xml:space="preserve">
  <value>Please enter valid dates to process attendance.</value>
  <comment>Zeeshan</comment>
  </data>
	<data name="RecordSavedSuccessfully" xml:space="preserve">
  <value>Record saved successfully.</value>
  <comment>Zeeshan</comment>
  </data>
  <data name="InvalidLeaveRequest" xml:space="preserve">
  <value>Leave from date can't be greater then leave to date.</value>
  <comment>Zeeshan</comment>
  </data>
  <data name="ColumnPopulatedSuccessfully" xml:space="preserve">
  <value>Columns name populated successfully.</value>
  <comment>Zeeshan</comment>
  </data>
  <data name="AttRecordImportedSuccessfully" xml:space="preserve">
  <value>Attendance records imported successfully.</value>
  <comment>Zeeshan</comment>
  </data>
  <data name="NoRecordFound" xml:space="preserve">
  <value>No Record(s) Found.</value>
  <comment>Zeeshan</comment>
  </data>
</root>