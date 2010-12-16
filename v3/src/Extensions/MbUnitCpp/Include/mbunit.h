// Copyright 2005-2010 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#pragma once
#pragma warning (disable: 4003)

namespace mbunit
{
	// A simple general purpose string container with formatting capabilities.
	class String
	{
		wchar_t* buffer;
		int length;
		void AppendImpl(const wchar_t* wstr, int n);
		void AppendImpl(const char* str);

		public:
		String();
		~String();
		String(const String& prototype);
		String(const char* str);
		String(const wchar_t* wstr);
		void Clear();
		template<typename T> String& Append(T arg);
		String& AppendFormat(const char* format, ...);
		String& AppendFormat(const wchar_t* format, ...);
		String& AppendFormat(const wchar_t* format, va_list args);
		String& AppendFormat(const char* format, va_list args);
		wchar_t* GetBuffer() { return buffer; }
		size_t GetLength() { return length; }

		public:
		static String Format(const char* format, ...);
		static String Format(const wchar_t* format, ...);
	};

	// A node that reference a mapped string value.
	struct StringMapNode
	{
		int Key;
		String* Str;
		StringMapNode* Next;
	};

	// Key type for the string map.
	#define StringId int

	// A simple list of strings (chained list)
	// TODO: Use a hash table for more efficiency.
	class StringMap
	{
		StringMapNode* head;
		StringId nextId;

		public:
		StringMap();
		~StringMap();
		String* Get(StringId key);
		StringId Add(String* str);
		void Remove(StringId key);
		void RemoveAll();
	};

    class Test;
    class TestList;
	class TestFixture;
    class TestFixtureList;

    // Standard outcome of a test.
    enum Outcome
    {
        Inconclusive = 0,
        Passed = 1,
        Failed = 2,
    };

	// The inner type of an actual/expected value.
	// Will be used by the Gallio test adapter to parse and represent the values properly.
	enum ValueType
	{
		// A raw string that represents a custom/user type.
		// Not parsed and displayed as it is.
		TypeRaw = 0,

		// A string type copied later in a System.String.
		// Displayed with diffing if both the actual and expected values are available.
		TypeString = 1,

		// A boolean type (should be "true" or "false")
		// Parsed by the test adapater with System.Boolean.Parse.
		TypeBoolean = 2,

		// A simple character. Parsed with System.Char.Parse.
		TypeChar = 3,

		// Primitive values parsed with the corresponding parsing method (System.Byte.Parse, System.Int16.Parse, etc.)
		TypeByte = 4,
		TypeInt16 = 5,
		TypeUInt16 = 6,
		TypeInt32 = 7,
		TypeUInt32 = 8,
		TypeInt64 = 9,
		TypeUInt64 = 10,
		TypeSingle = 11,
		TypeDouble = 12,
	};

    // Represents a single assertion failure.
	struct LabeledValue
	{
		StringId LabelId;
		StringId ValueId;
		ValueType ValueType;
		LabeledValue();
		void Set(StringId valueId, mbunit::ValueType valueType, StringId labelId = 0);
	};

    struct AssertionFailure
    {
        StringId DescriptionId;
        StringId MessageId;
		LabeledValue Expected;
        LabeledValue Actual;
        LabeledValue Extra_0;
        LabeledValue Extra_1;
		AssertionFailure();
		static AssertionFailure FromException(char* exceptionMessage = 0);
    };

    // Describes the result of a test.
    struct TestResultData
    {
        Outcome NativeOutcome;
        int AssertCount;
		AssertionFailure Failure;
		int TestLogId;
		int DurationMilliseconds;
	};

    // The MbUnitCpp Assertion Framework.
    class AssertionFramework
    {
        Test *test;
        void IncrementAssertCount();
		StringMap& Map() const;
		template<typename T> StringId AddNewStringFrom(T arg);
		StringId AddNewString(const char* str);
		StringId AddNewString(const wchar_t* wstr);
		StringId AddNewString(const String& str);

        public:
        AssertionFramework(Test* test);
        void Fail(const String& message);
		void Fail() { Fail(""); }
		void IsTrue(bool actualValue, const String& message);
		void IsTrue(bool actualValue) { IsTrue(actualValue, ""); }
		void IsFalse(bool actualValue, const String& message);
		void IsFalse(bool actualValue) { IsFalse(actualValue, ""); }
		template<typename T> void AreEqual(T expectedValue, T actualValue, const String& message);
		template<typename T> void AreEqual(T expectedValue, T actualValue) { AreEqual<T>(expectedValue, actualValue, ""); }
		template<typename T> void AreApproximatelyEqual(T expectedValue, T actualValue, T delta, const String& message);
		template<typename T> void AreApproximatelyEqual(T expectedValue, T actualValue, T delta) { AreApproximatelyEqual<T>(expectedValue, actualValue, delta, ""); }
    };

	// Provides an access to the Gallio test log.
	class TestLogRecorder
	{
        Test *test;

		public:
		TestLogRecorder(Test* test);
		void Write(const char* str);
		void Write(const wchar_t* wstr);
		void Write(const String& str);
		void WriteLine(const char* str);
		void WriteLine(const wchar_t* wstr);
		void WriteLine(const String& str);
		void WriteFormat(const char* format, ...);
		void WriteFormat(const wchar_t* format, ...);
		void WriteLineFormat(const char* format, ...);
		void WriteLineFormat(const wchar_t* format, ...);
	};

	// Base class for tests and test fixtures.
	class DecoratorTarget
	{
		private:
        int metadataId;

		protected:
		DecoratorTarget(int metadataPrototypeId = 0);
		void AppendTo(int& id, const String& s);
		virtual void SetMetadata(const wchar_t* key, const wchar_t* value);
		virtual void SetMetadata(const wchar_t* key, const char* value);
		void NoOp() const { }

		public:
		int GetMetadataId() const { return metadataId; }
	};
	
	class AbstractDataSource
	{
		private:
        void* head;
	    
		protected:
		AbstractDataSource();
		void SetHead(void* dataRow);

		public:
        void* GetHead() const { return head; }
		virtual void* GetNextRow(void* dataRow) = 0;
	};

    // Base class for executable tests.
    class Test : public DecoratorTarget
    {
        int index;
        const wchar_t* name;
        const wchar_t* fileName;
        int lineNumber;
        Test* next;
        int assertCount;
		int testLogId;
		AbstractDataSource* dataSource;

        public:
        Test(TestFixture* testFixture, const wchar_t* name, const wchar_t* fileName, int lineNumber);
        ~Test();
        int GetIndex() const { return index; }
        const wchar_t* GetName() const { return name; }
        const wchar_t* GetFileName() const { return fileName; }
        int GetLineNumber() const { return lineNumber; }
        Test* GetNext() const { return next; }
        void SetNext(Test* test);
        void Run(TestResultData* pTestResultData, void* dataRow);
        virtual void RunImpl();
        void IncrementAssertCount();
		void AppendToTestLog(const String& s);
		AbstractDataSource* GetDataSource() const { return dataSource; }

        private:
        void Clear();

        protected:
        AssertionFramework Assert;
		TestLogRecorder TestLog;
		void Bind(AbstractDataSource* dataSource);
		virtual void BindDataRow(void* dataRow);
    };

    // A chained list of tests.
    class TestList
    {
        private:
        Test* head;
        Test* tail;
        int nextIndex;
    
        public:
        TestList();
        void Add(Test* test);
        Test* GetHead() const { return head; }
        int GetNextIndex();
    };

    // A test fixture that defines a sequence of related child tests.
    class TestFixture : public DecoratorTarget
    {
        int index;
        const wchar_t* name;
        TestList children;
        TestFixture* next;

        public:
        TestFixture(int index, const wchar_t* name);
        ~TestFixture();
        int GetIndex() const { return index; }
        TestFixture* GetNext() const { return next; }
        void SetNext(TestFixture* pTestFixture);
        const wchar_t* GetName() const { return name; }
        TestList& GetTestList();
        static TestFixtureList& GetTestFixtureList();
		static StringMap& GetStringMap();
    };

    // A chained list of test fixtures
    class TestFixtureList
    {
        private:
        TestFixture* head;
        TestFixture* tail;
        int nextIndex;
    
        public:
        TestFixtureList();
        void Add(TestFixture* pTestFixture);
        TestFixture* GetHead() const { return head; }
        int GetNextIndex();
    };

    // Helper class to register a new test.
    class TestRecorder
    {
        public:
        TestRecorder(TestList& list, Test* pTest);
    };

    // Helper class to register a new test fixture.
    class TestFixtureRecorder
    {
        public:
        TestFixtureRecorder(TestFixtureList& list, TestFixture* pTestFixture);
    };
}

// Helper macros.
#define MBU_WSTR2(s) L##s
#define MBU_WSTR(s) MBU_WSTR2(s)
#define MBU_WFILE MBU_WSTR(__FILE__)
#define MBU_CONCAT(arg1, arg2) MBU_CONCAT1(arg1, arg2)
#define MBU_CONCAT1(arg1, arg2) MBU_CONCAT2(arg1, arg2)
#define MBU_CONCAT2(arg1, arg2) arg1##arg2
#define MBU_LAST_ARG(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, N, ...) N 
#define MBU_REVERSED_RANGE 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0
#define MBU_LP (
#define MBU_RP )
#define MBU_SC ;
#define MBU_C ,
#define MBU_COUNT_ARGS(...) MBU_LAST_ARG MBU_LP __VA_ARGS__##MBU_REVERSED_RANGE MBU_C MBU_REVERSED_RANGE MBU_RP
#define MBU_FOR_EACH_1(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP
#define MBU_FOR_EACH_2(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_1 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_3(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_2 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_4(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_3 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_5(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_4 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_6(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_5 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_7(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_6 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_8(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_7 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_9(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_8 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_10(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_9 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_11(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_10 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_12(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_11 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_13(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_12 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_14(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_13 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_15(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_14 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_16(_0, ...) MBU_PRINT_ARG MBU_LP _0 MBU_RP MBU_FOR_EACH_15 MBU_LP __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH_N(N, _0, ...) MBU_CONCAT MBU_LP MBU_FOR_EACH_, N MBU_RP MBU_LP _0, __VA_ARGS__ MBU_RP
#define MBU_FOR_EACH(_0, ...) MBU_FOR_EACH_N MBU_LP MBU_COUNT_ARGS MBU_LP _0 MBU_C __VA_ARGS__ MBU_RP MBU_C _0 MBU_C __VA_ARGS__ MBU_RP
#define MBU_PRINT_ARG(x) x MBU_SC

// Macro to create a new test fixture.
#define TESTFIXTURE(Name, ...) MBU_TESTFIXTURE MBU_LP Name, NoOp MBU_LP MBU_RP MBU_SC MBU_C __VA_ARGS__ MBU_RP
#define MBU_TESTFIXTURE(Name, _0, ...) \
    namespace NamespaceTestFixture##Name \
    { \
        class TestFixture##Name : public mbunit::TestFixture \
        { \
            public: \
            TestFixture##Name() : TestFixture(mbunit::TestFixture::GetTestFixtureList().GetNextIndex(), L#Name) { Decorate(); } \
			private: \
			void Decorate() { MBU_FOR_EACH MBU_LP _0 MBU_C __VA_ARGS__ MBU_RP } \
        } testFixtureInstance; \
        \
        mbunit::TestFixtureRecorder fixtureRecorder(mbunit::TestFixture::GetTestFixtureList(), &testFixtureInstance); \
    } \
    namespace NamespaceTestFixture##Name

// Macro to create a new test.
#define TEST(Name, ...) MBU_TEST MBU_LP Name, NoOp MBU_LP MBU_RP MBU_SC MBU_C __VA_ARGS__ MBU_RP
#define MBU_TEST(Name, _0, ...) \
    class Test##Name : public mbunit::Test \
    { \
        public: \
		Test##Name() : mbunit::Test(&testFixtureInstance, L#Name, MBU_WFILE, __LINE__) { Decorate(); } \
        private: \
		void Decorate() { MBU_FOR_EACH MBU_LP _0 MBU_C __VA_ARGS__ MBU_RP } \
        virtual void RunImpl(); \
    } test##Name##Instance; \
    mbunit::TestRecorder recorder##Name (testFixtureInstance.GetTestList(), &test##Name##Instance); \
    void Test##Name::RunImpl()

// Metadata decorators.
#define CATEGORY(category) SetMetadata(L"Category", category)
#define AUTHOR(authorName) SetMetadata(L"Author", authorName)
#define DESCRIPTION(description) SetMetadata(L"Description", description)

// Data source for data-driven tests.
#define DATA(name, _0, ...) \
    class DataSource##name : public mbunit::AbstractDataSource \
    { \
        public: \
        struct DataRow \
        { \
			struct DataRow* next; \
            MBU_FOR_EACH MBU_LP _0 MBU_C __VA_ARGS__ MBU_RP \
        }; \
        private: \
        struct DataRow* tail; \
        void Populate(); \
        void Add(const struct DataRow& dataRow) \
        { \
            struct DataRow* p = new struct DataRow(dataRow); \
            if (tail == 0) { SetHead(p); } else { tail->next = p; } \
            tail = p; \
        } \
        public: \
		virtual void* GetNextRow(void* dataRow) { return ((struct DataRow*)dataRow)->next; } \
        DataSource##name() : tail(0) { Populate(); } \
        ~DataSource##name() \
        { \
            struct DataRow* current = (struct DataRow*)GetHead(); \
            while (current != 0) { struct DataRow* next = current->next; delete current; current = next; } \
        } \
    }; \
    void DataSource##name::Populate()

// Data-driven test decorators.
#define ROW(...) do { struct DataRow t = { 0, __VA_ARGS__ }; Add(t); } while(0);
#define BIND(name, row) \
	Bind(new DataSource##name()); \
	Decorate2(); } \
	struct DataSource##name::DataRow row; \
	virtual void BindDataRow(void* dataRow) { row = *(DataSource##name::DataRow*)dataRow; } \
	void Decorate2() {
	