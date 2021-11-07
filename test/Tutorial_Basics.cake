(add-cakelisp-search-directory "runtime")
(import &comptime-only "ComptimeHelpers.cake" "CHelpers.cake")

(c-import "<stdio.h>")

(defmacro hello-from-macro ()
  (tokenize-push output
    (fprintf stderr "Hello from macro land!\n"))
  (return true))

(defmacro defcommand (command-name symbol arguments array &rest body any)

  (get-or-create-comptime-var command-table (<> (in std vector) (* (const Token))))
  (call-on-ptr push_back command-table command-name)

  (tokenize-push output
    (defun (token-splice command-name) (token-splice arguments)
      (token-splice-rest body tokens)))
  (return true))

(defcommand say-your-name ()
  (fprintf stderr "your name."))

(defun-comptime create-command-lookup-table (environment (& EvaluatorEnvironment)
                                             was-code-modified (& bool) &return bool)
  (get-or-create-comptime-var command-table (<> (in std vector) (* (const Token))))

  (var command-data (* (<> std::vector Token)) (new (<> std::vector Token)))
  (call-on push_back (field environment comptimeTokens) command-data)

  (for-in command-name (* (const Token)) (deref command-table)
    (printFormattedToken stderr (deref command-name))
    (fprintf stderr "\n")

    (var command-name-string Token (deref command-name))
    (set (field command-name-string type) TokenType_String)

    (tokenize-push (deref command-data)
      (array (token-splice-addr command-name-string)
             (token-splice command-name))))

  (prettyPrintTokens (deref command-data))

  (var command-table-tokens (* (<> std::vector Token)) (new (<> std::vector Token)))
  (call-on push_back (field environment comptimeTokens) command-table-tokens)
  (tokenize-push (deref command-table-tokens)
    (var command-table ([] command-metadata)
      (array (token-splice-array (deref command-data)))))
  (prettyPrintTokens (deref command-table-tokens))

  (return (ClearAndEvaluateAtSplicePoint environment "command-lookup-table" command-table-tokens)))

(add-compile-time-hook post-references-resolved
                       create-command-lookup-table)

;; Our command functions take no arguments and return nothing
(def-function-signature command-function ())

(defstruct-local command-metadata
  name (* (const char))
  command command-function)

(splice-point command-lookup-table)

(defun main (num-arguments int
             arguments ([] (* char))
             &return int)
  (fprintf stderr "Available commands:\n")
  (each-in-array command-table i
    (fprintf stderr "  %s\n"
             (field (at i command-table) name)))

  (unless (= 2 num-arguments)
    (fprintf stderr "Expected command argument\n")
    (return 1))
  (fprintf stderr "Hello, Cakelisp!\n")
  (hello-from-macro)
  (return 0))
