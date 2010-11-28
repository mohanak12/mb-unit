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

namespace MbUnitCpp
{
	// A simple general purpose string container with formatting capabilities.
	class String
	{
		wchar_t* data;
		void Initialize(const wchar_t* format, va_list argList);

		public:
		String(const String& prototype);
		String(const wchar_t* format, ...);
		String(const wchar_t* format, va_list args);
		~String();
		wchar_t* GetData() const { return data; };
		void Append(const String& string);
	};

	// A node that reference a mapped string value.
	struct StringMapNode
	{
		int Key;
		String* Data;
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
		void RemoveAll();
		String* Get(StringId key);
		StringId Add(String* data);
		StringId Add(const wchar_t* format, ...);
		StringId Add(const char* data);
		void Remove(StringId key);
	};

    class Test;
    class TestList;
	class TestFixture;
    class TestFixtureList;

    // Standard outcome of a test.
    enum Outcome
    {
        Inconclusive,
        Passed,
        Failed,
    };

	// The inner type of an actual/expected value.
	// Will be used by the Gallio test adapter to parse and represent the values properly.
	enum ValueType
	{
		// A raw string that represents a custom/user type.
		// Not parsed and displayed as it is.
		TypeRaw,

		// A string type copied later in a System.String.
		// Displayed with diffing if both the actual and expected values are available.
		TypeString,

		// A boolean type (should be "true" or "false")
		// Parsed by the test adapater with System.Boolean.Parse.
		TypeBoolean,

		// A simple character. Parsed with System.Char.Parse.
		TypeChar,

		// Primitive values parsed with the corresponding parsing method (System.Byte.Parse, System.Int16.Parse, etc.)
		TypeByte,
		TypeInt16,
		TypeUInt16,
		TypeInt32,
		TypeUInt32,
		TypeInt64,
		TypeUInt64,
		TypeSingle,
		TypeDouble,
	};

    // Represents a single assertion failure.
	struct LabeledValue
	{
		StringId LabelId;
		StringId ValueId;
		ValueType ValueType;
		LabeledValue();
		void Set(StringId valueId, MbUnitCpp::ValueType valueType, StringId labelId = 0);
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
    };

    // Describes the result of a test.
    struct TestResultData
    {
        Outcome NativeOutcome;
        int AssertCount;
		AssertionFailure Failure;
		int TestLogId;
	};

    // The MbUnitCpp Assertion Framework.
    class AssertionFramework
    {
        Test *test;
        void IncrementAssertCount();
		StringMap& Map() const;

        public:
        AssertionFramework(Test* test);

		// Outcome assertions.
        void Fail(const wchar_t* message = 0);

		// Logic assertions.
		void IsTrue(bool actualValue, const wchar_t* message = 0);
		void IsFalse(bool actualValue, const wchar_t* message = 0);
		void IsTrue(int actualValue, const wchar_t* message = 0); // Sometimes, boolean values are just int's (e.g. BOOL)
		void IsFalse(int actualValue, const wchar_t* message = 0);

		// Equality assertions.
		void AreEqual(bool expectedValue, bool actualValue, const wchar_t* message = 0);
		void AreEqual(char expectedValue, char actualValue, const wchar_t* message = 0);
		void AreEqual(wchar_t expectedValue, wchar_t actualValue, const wchar_t* message = 0);
		void AreEqual(unsigned char expectedValue, unsigned char actualValue, const wchar_t* message = 0);
		void AreEqual(short expectedValue, short actualValue, const wchar_t* message = 0);
		void AreEqual(unsigned short expectedValue, unsigned short actualValue, const wchar_t* message = 0);
		void AreEqual(int expectedValue, int actualValue, const wchar_t* message = 0);
		void AreEqual(unsigned int expectedValue, unsigned int actualValue, const wchar_t* message = 0);
		void AreEqual(long expectedValue, long actualValue, const wchar_t* message = 0);
		void AreEqual(unsigned long expectedValue, unsigned long actualValue, const wchar_t* message = 0);
		void AreEqual(long long expectedValue, long long actualValue, const wchar_t* message = 0);
		void AreEqual(unsigned long long expectedValue, unsigned long long actualValue, const wchar_t* message = 0);
		void AreEqual(float expectedValue, float actualValue, const wchar_t* message = 0);
		void AreEqual(double expectedValue, double actualValue, const wchar_t* message = 0);
		void AreEqual(char* expectedValue, char* actualValue, const wchar_t* message = 0);
		void AreEqual(const char* expectedValue, const char* actualValue, const wchar_t* message = 0);
		void AreEqual(wchar_t* expectedValue, wchar_t* actualValue, const wchar_t* message = 0);
		void AreEqual(const wchar_t* expectedValue, const wchar_t* actualValue, const wchar_t* message = 0);

		// Approximative equality assertions.
		void AreApproximatelyEqual(char expectedValue, char actualValue, char delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(wchar_t expectedValue, wchar_t actualValue, wchar_t delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(unsigned char expectedValue, unsigned char actualValue, unsigned char delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(short expectedValue, short actualValue, short delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(unsigned short expectedValue, unsigned short actualValue, unsigned short delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(int expectedValue, int actualValue, int delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(unsigned int expectedValue, unsigned int actualValue, unsigned int delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(long expectedValue, long actualValue, long delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(unsigned long expectedValue, unsigned long actualValue, unsigned long delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(long long expectedValue, long long actualValue, long long delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(unsigned long long expectedValue, unsigned long long actualValue, unsigned long long delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(float expectedValue, float actualValue, float delta, const wchar_t* message = 0);
		void AreApproximatelyEqual(double expectedValue, double actualValue, double delta, const wchar_t* message = 0);
    };

	// Provides an access to the Gallio test log.
	class TestLogRecorder
	{
        Test *test;

		public:
		TestLogRecorder(Test* test);
		void Write(const wchar_t* format, ...);
		void WriteLine(const wchar_t* format, ...);
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
#define MUC_WSTR2(s) L##s
#define MUC_WSTR(s) MUC_WSTR2(s)
#define MUC_WFILE MUC_WSTR(__FILE__)
#define MUC_CONCAT(arg1, arg2) MUC_CONCAT1(arg1, arg2)
#define MUC_CONCAT1(arg1, arg2) MUC_CONCAT2(arg1, arg2)
#define MUC_CONCAT2(arg1, arg2) arg1##arg2
#define MUC_LAST_ARG(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, N, ...) N 
#define MUC_REVERSED_RANGE 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0
#define MUC_LP (
#define MUC_RP )
#define MUC_SC ;
#define MUC_COUNT_ARGS(...) MUC_LAST_ARG MUC_LP __VA_ARGS__##MUC_REVERSED_RANGE, MUC_REVERSED_RANGE MUC_RP
#define MUC_FOR_EACH_1(Action, _0, ...) Action MUC_LP _0 MUC_RP
#define MUC_FOR_EACH_2(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_1 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_3(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_2 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_4(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_3 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_5(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_4 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_6(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_5 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_7(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_6 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_8(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_7 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_9(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_8 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_10(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_9 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_11(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_10 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_12(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_11 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_13(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_12 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_14(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_13 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_15(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_14 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_16(Action, _0, ...) Action MUC_LP _0 MUC_RP MUC_FOR_EACH_15 MUC_LP Action, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH_N(N, Action, _0, ...) MUC_CONCAT MUC_LP MUC_FOR_EACH_, N MUC_RP MUC_LP Action, _0, __VA_ARGS__ MUC_RP
#define MUC_FOR_EACH(Action, _0, ...) MUC_FOR_EACH_N MUC_LP MUC_COUNT_ARGS MUC_LP _0, __VA_ARGS__ MUC_RP , Action, _0, __VA_ARGS__ MUC_RP
#define MUC_PRINT_ARG(x) x;

//#define TOWSTR(pstr, buffer) \
//	size_t _len##buffer = mbstowcs(0, pstr, -1); \
//	wchar_t buffer[_len##buffer + 1]; \
//	mbstowcs(buffer, pstr, _len##buffer);

// Macro to create a new test fixture.
#define TESTFIXTURE(Name, ...) MUC_TESTFIXTURE MUC_LP Name, NoOp() MUC_SC, __VA_ARGS__ MUC_RP
#define MUC_TESTFIXTURE(Name, _0, ...) \
    using namespace MbUnitCpp; \
    namespace NamespaceTestFixture##Name \
    { \
        class TestFixture##Name : public TestFixture \
        { \
            public: \
            TestFixture##Name() : TestFixture(MbUnitCpp::TestFixture::GetTestFixtureList().GetNextIndex(), L#Name) { Decorate(); } \
			private: \
			void Decorate() { MUC_FOR_EACH MUC_LP MUC_PRINT_ARG, _0, __VA_ARGS__ MUC_RP } \
        } testFixtureInstance; \
        \
        MbUnitCpp::TestFixtureRecorder fixtureRecorder(MbUnitCpp::TestFixture::GetTestFixtureList(), &testFixtureInstance); \
    } \
    namespace NamespaceTestFixture##Name

// Macro to create a new test.
#define TEST(Name, ...) MUC_TEST MUC_LP Name, NoOp() MUC_SC, __VA_ARGS__ MUC_RP
#define MUC_TEST(Name, _0, ...) \
    class Test##Name : public MbUnitCpp::Test \
    { \
        public: \
		Test##Name() : Test(&testFixtureInstance, L#Name, MUC_WFILE, __LINE__) { Decorate(); } \
        private: \
		void Decorate() { MUC_FOR_EACH MUC_LP MUC_PRINT_ARG, _0, __VA_ARGS__ MUC_RP } \
        virtual void RunImpl(); \
    } test##Name##Instance; \
    MbUnitCpp::TestRecorder recorder##Name (testFixtureInstance.GetTestList(), &test##Name##Instance); \
    void Test##Name::RunImpl()

// Metadata decorators.
#define CATEGORY(category) SetMetadata(L"Category", L#category)
#define AUTHOR(authorName) SetMetadata(L"Author", L#authorName)
#define DESCRIPTION(description) SetMetadata(L"Description", L#description)

// Data source for data-driven tests.
#define DATA(name, _0, ...) \
    class DataSource##name : public AbstractDataSource \
    { \
        public: \
        struct DataRow \
        { \
			struct DataRow* next; \
            MUC_FOR_EACH MUC_LP MUC_PRINT_ARG, _0, __VA_ARGS__ MUC_RP \
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
	